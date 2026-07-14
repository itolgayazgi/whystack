// The PORT lives in `@whystack/api-client` (ADR-0022) — the website and the app must not have two ideas of
// what a refresh token store is.
//
// The WEB implementation lives there too, because it is the same in both: it stores nothing, because the
// token is in an HttpOnly cookie that JavaScript is not allowed to see (ADR-0008).
//
// Metro resolves `refresh-token-store.native.ts` instead of this file on iOS and Android, and THAT file is
// the one that talks to the Keychain. This is the reason the seam is a module rather than a runtime check.

export type { RefreshTokenStore, TokenPlatform } from '@whystack/api-client';
export { webRefreshTokenStore as refreshTokenStore } from '@whystack/api-client';
