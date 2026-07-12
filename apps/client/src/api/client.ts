import type { RefreshTokenStore, TokenPlatform } from '../auth/refresh-token-store';
import { ApiError, NetworkError, toApiError } from './problem';

export interface AuthTokens {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  /** Native only. On web it is null — the token is in an HttpOnly cookie this code cannot see. */
  refreshToken: string | null;
  refreshTokenExpiresAtUtc: string | null;
}

export interface ApiClientOptions {
  baseUrl: string;
  store: RefreshTokenStore;
  /** Called when the session ends for good — a refused refresh, a revoked family, a sign-out. */
  onSessionEnded: () => void;
  /** Called whenever a refresh succeeds, so state can pick up the new access token. */
  onTokensRenewed?: (tokens: AuthTokens) => void;
  fetchImpl?: typeof fetch;
}

/**
 * Requests that must NEVER trigger a refresh-and-retry.
 *
 * A 401 from `/auth/login` means the password was wrong — refreshing and retrying it would be
 * nonsense, and a 401 from `/auth/refresh` itself would recurse until the stack gave out.
 */
const NEVER_REFRESH = ['/api/v1/auth/login', '/api/v1/auth/register', '/api/v1/auth/refresh'];

export class ApiClient {
  /**
   * The access token lives HERE — in memory, in a private field, and nowhere else.
   *
   * Not `localStorage` (ADR-0008 says so outright: an XSS payload reads it), not AsyncStorage, not a
   * cookie this script can write. The cost is that a full page reload loses it — which is exactly why
   * `bootstrap()` exists: on launch we spend the refresh token, which IS stored properly, to get a new
   * access token. Fifteen minutes of memory-only lifetime is the whole design.
   */
  #accessToken: string | null = null;

  /**
   * The single in-flight refresh.
   *
   * **This field is the most important line in the file.** Refresh tokens ROTATE (ADR-0008): using one
   * invalidates it and issues a replacement, and presenting a rotated token again is treated as THEFT —
   * the server revokes the entire session family and signs the user out everywhere.
   *
   * Now picture the app resuming from background with three screens mounted. All three fire a request,
   * all three get a 401 because the access token expired while the phone was asleep, and all three call
   * `/auth/refresh` with the same stored token. The first rotates it. The other two present a token
   * that has just been rotated — and the server, correctly, concludes it has been stolen and kills
   * every session the user has.
   *
   * The user is signed out of every device, an alarm fires, and nothing was wrong. **We would have done
   * it to ourselves.**
   *
   * So: one refresh at a time. Everybody else awaits the same promise.
   */
  #refreshing: Promise<boolean> | null = null;

  readonly #baseUrl: string;
  readonly #store: RefreshTokenStore;
  readonly #onSessionEnded: () => void;
  readonly #onTokensRenewed?: (tokens: AuthTokens) => void;
  readonly #fetch: typeof fetch;

  constructor(options: ApiClientOptions) {
    this.#baseUrl = options.baseUrl.replace(/\/$/, '');
    this.#store = options.store;
    this.#onSessionEnded = options.onSessionEnded;
    this.#onTokensRenewed = options.onTokensRenewed;
    this.#fetch = options.fetchImpl ?? globalThis.fetch.bind(globalThis);
  }

  /**
   * Asked of the STORE, not of a module-level constant.
   *
   * The store is what actually knows: it is the thing that either holds the token or cannot. A separate
   * constant would be a second source for one fact, and the two would eventually disagree — in a test
   * first, silently, where a native-like fake store would still read the web constant and pass for the
   * wrong reason.
   */
  get platform(): TokenPlatform {
    return this.#store.platform;
  }

  get isSignedIn(): boolean {
    return this.#accessToken !== null;
  }

  /** After a login or a register-then-login: hold the access token, persist the refresh token. */
  async adopt(tokens: AuthTokens): Promise<void> {
    this.#accessToken = tokens.accessToken;

    // On web this is a no-op and the token is null — the cookie is already set. On native it goes to
    // the Keychain / Keystore.
    if (tokens.refreshToken !== null) {
      await this.#store.write(tokens.refreshToken);
    }
  }

  /**
   * Called once at launch: turn a stored refresh token (or a cookie) into a live session.
   *
   * Returns false when there is no session to restore — which is the normal state for a first-time
   * visitor, not an error.
   */
  async bootstrap(): Promise<boolean> {
    return this.#refreshOnce();
  }

  /** Ends the session on the SERVER, then locally. The order matters — see below. */
  async signOut(): Promise<void> {
    const refreshToken = await this.#store.read();

    try {
      // Server first. Clearing locally and skipping this would leave the refresh token VALID on the
      // server for thirty days — the user would believe they had signed out while the session they
      // thought they ended stayed live, usable by anyone who had already taken a copy.
      await this.request('/api/v1/auth/logout', {
        method: 'POST',
        body: { refreshToken, allDevices: false },
      });
    } catch {
      // A failed logout call must not trap the user in a session they have asked to leave. We clear
      // locally regardless — and this is the one place a swallowed error is right, because the
      // alternative is refusing to sign somebody out because the network is down.
      //
      // It is not silent: the session may still be live server-side, and the honest answer to that is
      // the short access-token lifetime and the fact that the refresh token is now gone from this
      // device. Nothing here pretends the server-side revocation succeeded.
    }

    this.#accessToken = null;
    await this.#store.clear();
    this.#onSessionEnded();
  }

  /**
   * One request, with the access token attached, and one retry after a refresh if it 401s.
   */
  async request<T>(
    path: string,
    options: { method?: string; body?: unknown; retryOn401?: boolean } = {},
  ): Promise<T> {
    const { method = 'GET', body, retryOn401 = true } = options;

    const response = await this.#send(path, method, body);

    // Retry ONCE, and only once. `retryOn401: false` on the retry is what stops a server that answers
    // 401 to everything from turning into an infinite loop of refreshes.
    const shouldRefresh = response.status === 401 && retryOn401 && !NEVER_REFRESH.includes(path);

    if (shouldRefresh) {
      const renewed = await this.#refreshOnce();

      if (!renewed) {
        // The refresh was refused: the session is over. Report it as the 401 it is, after the state
        // has already been told to sign out — so the UI shows the sign-in screen rather than an error
        // toast about a request nobody made on purpose.
        throw await toApiError(response);
      }

      return this.request<T>(path, { ...options, retryOn401: false });
    }

    if (!response.ok) {
      throw await toApiError(response);
    }

    return this.#readBody<T>(response);
  }

  /**
   * The single-flight refresh. Concurrent callers all await the SAME promise and share its result.
   */
  async #refreshOnce(): Promise<boolean> {
    // `??=` is the whole guard. The second caller finds a promise already there and awaits it instead
    // of starting a second rotation.
    this.#refreshing ??= this.#refresh().finally(() => {
      // Cleared as it settles, so the NEXT expiry starts a fresh refresh rather than replaying this
      // one's result forever.
      this.#refreshing = null;
    });

    return this.#refreshing;
  }

  async #refresh(): Promise<boolean> {
    const refreshToken = await this.#store.read();

    // On native, no stored token means no session. On web, read() always returns null and the cookie
    // does the talking — so we must still make the call and let the server decide.
    if (this.#store.platform === 'Native' && refreshToken === null) {
      return false;
    }

    let response: Response;

    try {
      response = await this.#send('/api/v1/auth/refresh', 'POST', {
        refreshToken,
        platform: this.#store.platform,
      });
    } catch (error) {
      // The network is down. This is NOT a refused refresh, and must not end the session: a user in a
      // lift should not be signed out and made to type their password again when they come back up.
      // Rethrow, and let the caller show an offline state.
      throw error instanceof NetworkError ? error : new NetworkError(error);
    }

    if (!response.ok) {
      // The server refused: expired, revoked, replayed. Whatever the reason, this token is worthless —
      // keeping it would mean retrying it on every launch, failing every time, and looking like a bug.
      this.#accessToken = null;
      await this.#store.clear();
      this.#onSessionEnded();

      return false;
    }

    const tokens = await this.#readBody<AuthTokens>(response);

    await this.adopt(tokens);
    this.#onTokensRenewed?.(tokens);

    return true;
  }

  async #send(path: string, method: string, body?: unknown): Promise<Response> {
    const headers: Record<string, string> = { Accept: 'application/json' };

    if (body !== undefined) {
      headers['Content-Type'] = 'application/json';
    }

    if (this.#accessToken !== null) {
      headers.Authorization = `Bearer ${this.#accessToken}`;
    }

    try {
      return await this.#fetch(`${this.#baseUrl}${path}`, {
        method,
        headers,
        body: body === undefined ? undefined : JSON.stringify(body),

        // What makes the HttpOnly refresh cookie travel at all. Without it the browser sends a
        // cross-origin request with no cookies, the refresh 401s, and every web user is signed out
        // fifteen minutes after signing in — with nothing in any log explaining why.
        //
        // It is inert on native, where there is no cookie jar.
        credentials: 'include',
      });
    } catch (error) {
      // fetch rejects only on a NETWORK failure — a 500 is a resolved promise. So anything landing here
      // means the request never reached the API, and that is a distinct state the UI owes the user.
      throw new NetworkError(error);
    }
  }

  async #readBody<T>(response: Response): Promise<T> {
    if (response.status === 204) {
      return undefined as T;
    }

    return (await response.json()) as T;
  }
}

export { ApiError, NetworkError };
