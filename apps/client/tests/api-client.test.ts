import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ApiClient } from '../src/api/client';
import { ApiError, NetworkError } from '../src/api/problem';
import type { RefreshTokenStore } from '../src/auth/refresh-token-store';

// A store that behaves like the NATIVE one — it actually holds a token — so these tests can exercise
// the path where the client owns the refresh token. The web store returns null by design (the token is
// in an HttpOnly cookie), which is tested separately in refresh-token-store.test.ts.
function fakeStore(initial: string | null = null): RefreshTokenStore & { token: string | null } {
  return {
    token: initial,
    async read() {
      return this.token;
    },
    async write(token: string) {
      this.token = token;
    },
    async clear() {
      this.token = null;
    },
  };
}

function jsonResponse(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  });
}

const TOKENS = {
  accessToken: 'access-2',
  accessTokenExpiresAtUtc: '2026-07-12T10:15:00Z',
  refreshToken: 'refresh-2',
  refreshTokenExpiresAtUtc: '2026-08-11T10:00:00Z',
};

describe('ApiClient', () => {
  let store: ReturnType<typeof fakeStore>;
  let onSessionEnded: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    store = fakeStore('refresh-1');
    onSessionEnded = vi.fn();
  });

  function clientWith(fetchImpl: typeof fetch) {
    return new ApiClient({
      baseUrl: 'https://api.test',
      store,
      onSessionEnded,
      fetchImpl,
    });
  }

  describe('the single-flight refresh', () => {
    /**
     * THE test in this file.
     *
     * Refresh tokens rotate: using one invalidates it, and presenting a rotated token again is treated
     * as theft — the server revokes the whole session family and signs the user out everywhere.
     *
     * So three screens waking up together, all 401ing, all refreshing, would rotate the same token
     * three times. The first succeeds. The other two replay a token that has just been rotated, the
     * server concludes it was stolen, and the user is signed out of every device they own.
     *
     * Nothing was wrong. We would have done it to ourselves. This test is what says we do not.
     */
    it('rotates the refresh token exactly once, no matter how many requests 401 at the same time', async () => {
      let refreshCalls = 0;

      const fetchImpl = vi.fn(async (input: RequestInfo | URL) => {
        const url = String(input);

        if (url.endsWith('/auth/refresh')) {
          refreshCalls += 1;

          // A real refresh is a network round trip. Resolving instantly would let each caller finish
          // before the next one started, and the race this test exists to catch would never happen —
          // the test would pass with the guard deleted.
          await new Promise((resolve) => setTimeout(resolve, 10));

          return jsonResponse(200, TOKENS);
        }

        // Every protected call 401s until the access token is renewed.
        return refreshCalls === 0
          ? jsonResponse(401, { code: 'authentication_required' })
          : jsonResponse(200, { ok: true });
      }) as unknown as typeof fetch;

      const client = clientWith(fetchImpl);

      // Three screens, one resumed app, one expired access token.
      const results = await Promise.all([
        client.request('/api/v1/users/me'),
        client.request('/api/v1/users/me/preferences'),
        client.request('/api/v1/topics'),
      ]);

      expect(refreshCalls).toBe(1);
      expect(results).toEqual([{ ok: true }, { ok: true }, { ok: true }]);
      expect(onSessionEnded).not.toHaveBeenCalled();

      // And all three retried and succeeded — the guard must not make the losers give up.
      expect(store.token).toBe('refresh-2');
    });

    it('starts a fresh refresh the next time the token expires', async () => {
      let refreshCalls = 0;
      let expired = true;

      const fetchImpl = vi.fn(async (input: RequestInfo | URL) => {
        const url = String(input);

        if (url.endsWith('/auth/refresh')) {
          refreshCalls += 1;
          expired = false;
          return jsonResponse(200, TOKENS);
        }

        return expired ? jsonResponse(401, { code: 'x' }) : jsonResponse(200, { ok: true });
      }) as unknown as typeof fetch;

      const client = clientWith(fetchImpl);

      await client.request('/api/v1/users/me');
      expect(refreshCalls).toBe(1);

      // Fifteen minutes later, it expires again. The in-flight promise must have been CLEARED — if it
      // were cached, this would replay the first result forever and the session would die silently
      // when that access token expired too.
      expired = true;
      await client.request('/api/v1/users/me');

      expect(refreshCalls).toBe(2);
    });
  });

  describe('when the refresh is refused', () => {
    it('ends the session, throws away the worthless token, and says so once', async () => {
      const fetchImpl = vi.fn(async (input: RequestInfo | URL) => {
        const url = String(input);

        return url.endsWith('/auth/refresh')
          ? jsonResponse(401, { code: 'invalid_refresh_token' })
          : jsonResponse(401, { code: 'authentication_required' });
      }) as unknown as typeof fetch;

      const client = clientWith(fetchImpl);

      await expect(client.request('/api/v1/users/me')).rejects.toBeInstanceOf(ApiError);

      expect(onSessionEnded).toHaveBeenCalledTimes(1);

      // The token is gone. Keeping a token the server has refused means retrying it on every launch,
      // failing every time, and looking like a bug in the app.
      expect(store.token).toBeNull();
      expect(client.isSignedIn).toBe(false);
    });
  });

  describe('offline', () => {
    /**
     * A user in a lift still has a valid session. Signing them out because the network blinked would
     * make them type their password again for nothing — so a network failure is NOT a refused refresh,
     * and must not end the session.
     */
    it('does not sign anybody out just because the network is down', async () => {
      const fetchImpl = vi.fn(async () => {
        throw new TypeError('Failed to fetch');
      }) as unknown as typeof fetch;

      const client = clientWith(fetchImpl);

      await expect(client.bootstrap()).rejects.toBeInstanceOf(NetworkError);

      expect(onSessionEnded).not.toHaveBeenCalled();

      // Still there, ready for the retry the UI offers.
      expect(store.token).toBe('refresh-1');
    });

    it('reports a dead network as a NetworkError, not as an API rejection', async () => {
      const fetchImpl = vi.fn(async () => {
        throw new TypeError('Failed to fetch');
      }) as unknown as typeof fetch;

      const client = clientWith(fetchImpl);

      // The UI shows "check your connection" for one of these and "that password is wrong" for the
      // other. Collapsing them into one error is how an app teaches people to distrust it.
      await expect(client.request('/api/v1/users/me')).rejects.toBeInstanceOf(NetworkError);
    });
  });

  describe('the retry', () => {
    it('never refreshes for a failed sign-in', async () => {
      let refreshCalls = 0;

      const fetchImpl = vi.fn(async (input: RequestInfo | URL) => {
        if (String(input).endsWith('/auth/refresh')) {
          refreshCalls += 1;
          return jsonResponse(200, TOKENS);
        }

        return jsonResponse(401, { code: 'invalid_credentials' });
      }) as unknown as typeof fetch;

      const client = clientWith(fetchImpl);

      const failure = await client
        .request('/api/v1/auth/login', { method: 'POST', body: { email: 'a@b.c', password: 'no' } })
        .catch((error: unknown) => error);

      // A 401 from login means the password was wrong. Refreshing and retrying it would be nonsense —
      // and would burn a rotation on every typo.
      expect(refreshCalls).toBe(0);
      expect(failure).toBeInstanceOf(ApiError);
      expect((failure as ApiError).code).toBe('invalid_credentials');
    });

    it('gives up after one retry rather than looping forever', async () => {
      let refreshCalls = 0;
      let protectedCalls = 0;

      const fetchImpl = vi.fn(async (input: RequestInfo | URL) => {
        if (String(input).endsWith('/auth/refresh')) {
          refreshCalls += 1;
          return jsonResponse(200, TOKENS);
        }

        // A server that answers 401 to everything, even with a brand-new token. Without a retry limit
        // this is an infinite loop that rotates the refresh token until the server kills the family.
        protectedCalls += 1;
        return jsonResponse(401, { code: 'authentication_required' });
      }) as unknown as typeof fetch;

      const client = clientWith(fetchImpl);

      await expect(client.request('/api/v1/users/me')).rejects.toBeInstanceOf(ApiError);

      expect(refreshCalls).toBe(1);
      expect(protectedCalls).toBe(2); // the original, and one retry. Not three, not infinity.
    });
  });

  describe('the access token', () => {
    it('is attached to protected requests and held only in memory', async () => {
      const seen: (string | null)[] = [];

      const fetchImpl = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
        const headers = new Headers(init?.headers);
        seen.push(headers.get('Authorization'));

        return String(input).endsWith('/auth/refresh')
          ? jsonResponse(200, TOKENS)
          : jsonResponse(200, { ok: true });
      }) as unknown as typeof fetch;

      const client = clientWith(fetchImpl);

      await client.adopt({ ...TOKENS, accessToken: 'access-1' });
      await client.request('/api/v1/users/me');

      expect(seen).toContain('Bearer access-1');

      // The store holds the REFRESH token and nothing else. An access token in the Keychain would be a
      // second copy of a credential that is supposed to die with the process.
      expect(store.token).toBe('refresh-2');
      expect(JSON.stringify(store)).not.toContain('access-1');
    });

    it('sends the cookie on every request, or the web session dies fifteen minutes after sign-in', async () => {
      let credentials: RequestCredentials | undefined;

      const fetchImpl = vi.fn(async (_input: RequestInfo | URL, init?: RequestInit) => {
        credentials = init?.credentials;
        return jsonResponse(200, { ok: true });
      }) as unknown as typeof fetch;

      await clientWith(fetchImpl).request('/api/v1/users/me');

      expect(credentials).toBe('include');
    });
  });
});
