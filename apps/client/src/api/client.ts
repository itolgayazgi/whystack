// The API client lives in `@whystack/api-client` now (ADR-0022): the website and the mobile app share one
// implementation of the single-flight refresh guard, the Problem Details mapping and every endpoint's shape.
// A bug in any of them is fixed once.
//
// This file stays as a re-export so the app's imports do not all have to change at once — and so the module
// path that Metro resolves is still inside the app, which is what makes `refresh-token-store.native.ts`
// selectable by platform.

export type { ApiClientOptions, AuthTokens } from '@whystack/api-client';
export { ApiClient } from '@whystack/api-client';
