/**
 * Where the refresh token lives — the PORT, not an implementation.
 *
 * ADR-0008 gives two answers, and they are not interchangeable:
 *
 * - **Web** — an HttpOnly cookie. JavaScript cannot read it, write it or delete it. That is the entire
 *   point: a token JavaScript can reach is a token an XSS payload can steal, and the refresh token is the
 *   one credential worth stealing — whoever holds it can mint access tokens until it expires. The web store
 *   therefore stores *nothing*, and that is correct behaviour rather than a stub.
 *
 * - **Native** — the iOS Keychain / Android Keystore, via expo-secure-store. A phone has no cookie jar.
 *
 * **Never both.** Handing a browser a body copy of the token the cookie exists to hide would defeat the
 * cookie entirely.
 *
 * The interface lives here so that `packages/api-client` can be shared by `apps/web` and `apps/client`
 * (ADR-0022) without either one learning what the other's storage is.
 */

/** Which half of ADR-0008's token strategy a store implements. */
export type TokenPlatform = 'Web' | 'Native';

export interface RefreshTokenStore {
  /**
   * Which half of ADR-0008 this store is — and therefore what the API must be asked for.
   *
   * A Web caller must have its refresh token left in the HttpOnly cookie and NOT returned in the body; a
   * Native caller must have it in the body, because it has no cookie jar.
   *
   * It lives ON THE STORE rather than as a module-level constant, and that is not cosmetic. The store is
   * injected; a constant is not. With two sources for one fact, a test that passes a native-like store would
   * still read the web constant — and would pass by accident, proving nothing. Asking the store makes the
   * two impossible to disagree. (CodeQL found the first version of that bug.)
   */
  readonly platform: TokenPlatform;

  /** Web always returns null: the token exists, but in a place this code is not allowed to see. */
  read(): Promise<string | null>;
  write(token: string): Promise<void>;
  clear(): Promise<void>;
}

/**
 * The web store. It stores nothing, on purpose.
 *
 * Written out rather than left as an empty object so that nobody "fixes" it later by reaching for
 * `localStorage` — which would put the refresh token exactly where the cookie exists to keep it out of.
 *
 * The browser attaches the cookie to `/api/v1/auth/*` by itself, provided the request is sent with
 * `credentials: 'include'`. That is the whole client-side mechanism.
 */
export const webRefreshTokenStore: RefreshTokenStore = {
  platform: 'Web',

  read: () => Promise.resolve(null),
  write: () => Promise.resolve(),
  clear: () => Promise.resolve(),
};
