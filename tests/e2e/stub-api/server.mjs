#!/usr/bin/env node
/**
 * A stand-in for the WhyStack API, for the NATIVE end-to-end runs only.
 *
 * ─────────────────────────────────────────────────────────────────────────────────────────────────
 *  WHAT THIS TESTS, AND WHAT IT DOES NOT
 * ─────────────────────────────────────────────────────────────────────────────────────────────────
 *
 *  It tests the CLIENT on a real Android and a real iOS device: that the app builds, boots, resolves
 *  its platform-specific modules, loads its fonts, renders, navigates — and, above all, that
 *  `expo-secure-store` actually persists the refresh token in the iOS Keychain / Android Keystore, so
 *  that a session survives the app being killed. That last one has NO web equivalent. Not one of the
 *  60 web tests touches it, because the web build never loads `refresh-token-store.native.ts` at all.
 *
 *  It does NOT test the API. The API's contract is covered by 40 endpoint tests running against a real
 *  SQL Server, and it is not re-litigated here.
 *
 *  WHY A STUB AND NOT THE REAL API: GitHub's macOS runners have no Docker, and SQL Server does not run
 *  on macOS at all. The iOS job therefore cannot stand up the real backend. Running the real API on
 *  Android and a stub on iOS would give the two platforms different tests, so any divergence between
 *  them would be a test artefact rather than a real finding. One stub, both platforms, identical flows.
 *
 *  THE RISK THIS ACCEPTS, STATED PLAINLY: a stub can drift from the real API and the flows would stay
 *  green while the app was broken against the real thing. That is mitigated, not eliminated — the
 *  contract is asserted against the real API elsewhere, and the shapes here are copied from it. It is
 *  a real gap and it is filed, not hidden.
 * ─────────────────────────────────────────────────────────────────────────────────────────────────
 *
 * State is in memory and dies with the process. Every CI run starts from nothing, which is the only
 * way a flow that registers an account can be run twice.
 */
import { createServer } from 'node:http';

/**
 * Fixed, not configurable.
 *
 * It has to match `EXPO_PUBLIC_API_URL`, which is baked into the JavaScript bundle at BUILD time — so a
 * port that could be overridden at runtime would be a port that could silently disagree with the app
 * that is trying to reach it. The failure would look like "the app cannot connect", which is also what
 * a genuine networking bug looks like.
 */
const PORT = 5207;

/** email → { password, displayName, confirmed } */
const users = new Map();

/** refreshToken → { email, rotated } — the rotation chain, modelled because the CLIENT depends on it. */
const sessions = new Map();

let counter = 0;
const nextToken = (kind) => `${kind}-${++counter}`;

const DEFAULT_PREFERENCES = {
  applicationLanguage: 'en',
  contentLanguage: 'en',
  themeMode: 'System',
  readingFontScale: 1.0,
  reducedMotionEnabled: false,
  preferredSkillLevel: null,
  rowVersion: 'AAAAAAAAAAE=',
};

/** email → preferences */
const preferences = new Map();

/**
 * Every request, with its outcome — and NEVER a password, a token or a body.
 *
 * On a platform nobody here can run locally, this log is most of the debugging information there is.
 * The iOS flow failed with a generic "something went wrong" and an empty-looking password field, and
 * there was no way to tell whether the app had even reached the server, what it sent, or what came
 * back. Guessing at that from a screenshot is how you fix the wrong thing twice.
 *
 * The password's LENGTH is logged; the password is not. That distinction is the whole point: it answers
 * "did the text actually land in the field?" without writing a credential into a CI log that is public,
 * retained, and indexed (`12` logging rules — never log a secret, and a test's log is still a log).
 */
function log(request, status, note = '') {
  process.stdout.write(
    `${request.method} ${(request.url ?? '').split('?')[0]} → ${status}${note ? `  ${note}` : ''}\n`,
  );
}

function problem(response, status, code, detail) {
  response.writeHead(status, { 'Content-Type': 'application/problem+json' });
  response.end(JSON.stringify({ status, code, detail, title: code }));
}

function json(response, status, body) {
  response.writeHead(status, { 'Content-Type': 'application/json' });
  response.end(JSON.stringify(body));
}

function issueSession(email) {
  const refreshToken = nextToken('refresh');
  sessions.set(refreshToken, { email, rotated: false });

  return {
    accessToken: `access-for-${email}-${counter}`,
    accessTokenExpiresAtUtc: new Date(Date.now() + 15 * 60_000).toISOString(),
    // Native gets the token in the BODY (ADR-0008). The stub does not set a cookie, and it does not
    // need to: the whole point of these runs is the platform where there is no cookie jar.
    refreshToken,
    refreshTokenExpiresAtUtc: new Date(Date.now() + 30 * 86_400_000).toISOString(),
  };
}

function userOf(request) {
  const header = request.headers.authorization ?? '';
  const match = /^Bearer access-for-(.+)-\d+$/.exec(header);

  return match?.[1] ?? null;
}

const server = createServer((request, response) => {
  let raw = '';
  request.on('data', (chunk) => {
    raw += chunk;
  });

  request.on('end', () => {
    const body = raw.length > 0 ? JSON.parse(raw) : {};
    const path = (request.url ?? '').split('?')[0];
    const method = request.method ?? 'GET';

    // So CI can wait for the process to be listening rather than sleeping and hoping. A fixed sleep is
    // how a suite becomes flaky on a slow runner and slow on a fast one.
    if (path === '/health') {
      return json(response, 200, { status: 'Healthy' });
    }

    // ── auth ──────────────────────────────────────────────────────────────────────────────────────
    if (path === '/api/v1/auth/register' && method === 'POST') {
      // The same answer either way — the account-enumeration defence (`04`). The flow asserts the
      // screen shows one message; a stub that leaked here would let a leaking client pass.
      if (!users.has(body.email)) {
        users.set(body.email, {
          password: body.password,
          displayName: body.displayName ?? null,
          // `04` Device Language Detection: a Turkish device starts the account in Turkish.
          confirmed: false,
        });

        preferences.set(body.email, {
          ...DEFAULT_PREFERENCES,
          applicationLanguage: String(body.deviceLocale ?? '')
            .toLowerCase()
            .startsWith('tr')
            ? 'tr'
            : 'en',
          contentLanguage: String(body.deviceLocale ?? '')
            .toLowerCase()
            .startsWith('tr')
            ? 'tr'
            : 'en',
        });
      }

      log(
        request,
        202,
        `email=${body.email} passwordLength=${String(body.password ?? '').length} locale=${body.deviceLocale}`,
      );

      return json(response, 202, {
        message: 'If that address can be registered, we have sent it a confirmation email.',
      });
    }

    if (path === '/api/v1/auth/login' && method === 'POST') {
      const user = users.get(body.email);

      // The two facts that matter, and neither of them is the password itself: did the text land in the
      // field, and did it match what registration stored?
      log(
        request,
        user && user.password === body.password ? 200 : 401,
        `email=${body.email} passwordLength=${String(body.password ?? '').length} known=${Boolean(user)} platform=${body.platform}`,
      );

      if (!user || user.password !== body.password) {
        return problem(response, 401, 'invalid_credentials', 'Authentication failed.');
      }

      return json(response, 200, {
        ...issueSession(body.email),
        user: {
          id: body.email,
          email: body.email,
          displayName: user.displayName,
          isEmailConfirmed: user.confirmed,
          createdAtUtc: new Date().toISOString(),
          roles: ['RegisteredUser'],
        },
      });
    }

    if (path === '/api/v1/auth/refresh' && method === 'POST') {
      const session = sessions.get(body.refreshToken);

      log(
        request,
        session ? 200 : 401,
        `hasToken=${Boolean(body.refreshToken)} known=${Boolean(session)} platform=${body.platform}`,
      );

      if (!session) {
        return problem(response, 401, 'invalid_refresh_token', 'Session ended.');
      }

      // Rotation and reuse detection, modelled — because the CLIENT's single-flight guard exists
      // entirely to avoid tripping it. A stub that handed out the same token forever would let a
      // client with no guard sail through, and the very bug this defends against would ship.
      if (session.rotated) {
        for (const [token, entry] of sessions) {
          if (entry.email === session.email) sessions.delete(token);
        }

        return problem(response, 401, 'invalid_refresh_token', 'Session ended.');
      }

      session.rotated = true;

      return json(response, 200, issueSession(session.email));
    }

    if (path === '/api/v1/auth/logout' && method === 'POST') {
      sessions.delete(body.refreshToken);

      return json(response, 200, { message: 'Signed out.' });
    }

    // ── users ─────────────────────────────────────────────────────────────────────────────────────
    const email = userOf(request);

    if (email === null || !users.has(email)) {
      return problem(response, 401, 'authentication_required', 'Authentication required.');
    }

    if (path === '/api/v1/users/me' && method === 'GET') {
      const user = users.get(email);

      return json(response, 200, {
        id: email,
        email,
        displayName: user.displayName,
        isEmailConfirmed: user.confirmed,
        createdAtUtc: new Date().toISOString(),
        roles: ['RegisteredUser'],
      });
    }

    if (path === '/api/v1/users/me/preferences') {
      if (method === 'GET') {
        log(request, 200, `email=${email}`);

        return json(response, 200, preferences.get(email) ?? DEFAULT_PREFERENCES);
      }

      if (method === 'PUT') {
        const current = preferences.get(email) ?? DEFAULT_PREFERENCES;

        // Optimistic concurrency, modelled. The client sends the rowVersion it read; a stale one is a
        // 409. Accepting anything would let a client that dropped the rowVersion pass.
        if (body.rowVersion !== current.rowVersion) {
          return problem(response, 409, 'concurrency_conflict', 'Changed somewhere else.');
        }

        // A FRESH rowVersion on every write — exactly what SQL Server does. A stub that echoed the old
        // one back would let a client that ignores the response pass, and then that client would 409
        // against its own second save in production.
        const saved = {
          ...body,
          rowVersion: Buffer.from(String(++counter).padStart(8, '0')).toString('base64'),
        };

        preferences.set(email, saved);
        log(request, 200, `email=${email} theme=${saved.themeMode}`);

        return json(response, 200, saved);
      }
    }

    log(request, 404);

    return problem(response, 404, 'resource_not_found', `No route for ${method} ${path}.`);
  });
});

server.listen(PORT, '0.0.0.0', () => {
  process.stdout.write(`stub API listening on 0.0.0.0:${PORT}\n`);
});
