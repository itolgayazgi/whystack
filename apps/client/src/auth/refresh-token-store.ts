/**
 * Where the refresh token lives — the WEB answer.
 *
 * On web the refresh token is in an **HttpOnly cookie**, which means JavaScript cannot read it, cannot
 * write it, and cannot delete it. That is the entire point (ADR-0008): a token JavaScript can reach is
 * a token an XSS payload can steal, and the refresh token is the one credential worth stealing —
 * whoever holds it can mint access tokens until it expires.
 *
 * So this implementation stores nothing, and that is not a stub or a TODO. It is the correct
 * behaviour, and it is written out rather than left as an empty object so that nobody "fixes" it later
 * by reaching for `localStorage`.
 *
 * The browser attaches the cookie to `/api/v1/auth/*` by itself, provided the request is sent with
 * `credentials: 'include'`. That is the whole client-side mechanism.
 *
 * Metro resolves `refresh-token-store.native.ts` on iOS and Android instead of this file.
 */
export interface RefreshTokenStore {
  /** Web always returns null: the token exists, but in a place this code is not allowed to see. */
  read(): Promise<string | null>;
  write(token: string): Promise<void>;
  clear(): Promise<void>;
}

/**
 * True when the platform keeps the refresh token somewhere this code can read it.
 *
 * Callers need to know, because it changes what they must do. On web, `POST /auth/refresh` sends no
 * body and relies on the cookie; on native it must put the token in the body. Handing out BOTH would
 * give the web client a JavaScript-readable copy of the very token the cookie exists to hide.
 */
export const refreshTokenIsReadable = false;

export const refreshTokenStore: RefreshTokenStore = {
  // Never localStorage. Never sessionStorage. Never a cookie this script can write.
  async read() {
    return null;
  },

  async write() {
    // Deliberately nothing. The API already set the cookie in its Set-Cookie response header, and the
    // browser stored it. There is nothing here left to do, and anything this function did store would
    // be a second copy in a place an attacker can reach.
  },

  async clear() {
    // Also deliberately nothing. Signing out is a SERVER action: POST /auth/logout revokes the session
    // and sends back a Set-Cookie that expires it. Clearing it here is not possible (HttpOnly) and
    // would be a lie if it appeared to succeed — the session would still be live on the server.
  },
};
