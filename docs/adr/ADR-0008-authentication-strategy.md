# ADR-0008 — Authentication Strategy

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Sources: Confirmed decision (2026-07-11); `05-system-architecture.md`, `07-database-design.md`, `08-api-standards.md`, `12-engineering-standards.md`, `13-quality-assurance.md`

---

## Context

`05`, `08` and `12` all described authentication as "token **or** cookie/session," leaving the mechanism unresolved. Sprint 2 (Identity) cannot be built without a concrete decision, and it shapes `07`'s `UserSessions` model. This ADR covers *authentication* only; *authorization* (roles) is ADR-0005.

## Decision

1. **Mechanism:** short-lived **JWT access token** + **rotating refresh token**.
   - Access token lifetime: short (target ~15 minutes).
   - Refresh token: long-lived, **rotated on every use**, with **reuse detection** (a replayed refresh token revokes the session family).
2. **Token storage:**
   - **Mobile (Android/iOS):** refresh token in secure device storage — **iOS Keychain / Android Keystore**; access token held in memory.
   - **Web (RN-Web):** refresh token in an **HttpOnly, Secure, SameSite** cookie; access token held in memory (never `localStorage`).
3. **Server persistence:** only the **hash** of the refresh token is stored, in `UserSessions.RefreshTokenHash` (already defined in `07`). Raw tokens are never stored or logged (per `12` logging rules).
4. **Endpoints:** as defined in `08` — `/auth/register, /auth/login, /auth/logout, /auth/refresh, /auth/confirm-email, /auth/forgot-password, /auth/reset-password`.
5. **Controls:** rate limiting on auth endpoints, account-enumeration protection, session revocation, and audit logging of identity events (`UserLoginEvents`), per `07`/`12`/`13`.
6. **CSRF:** because the web refresh cookie is used, CSRF protection applies to cookie-authenticated state-changing requests (per `08` security standard).

## Alternatives Considered

- **Cookie/session everywhere.** Rejected: session cookies are awkward and inconsistent to manage in a React Native mobile client; weakens cross-platform consistency.
- **JWT bearer everywhere (no cookie).** Rejected: storing a long-lived token in web-accessible storage exposes it to XSS theft. The HttpOnly cookie for the refresh token on web is materially safer.

## Consequences

- `07` `UserSessions` already supports this (RefreshTokenHash, ExpiresAtUtc, RevokedAtUtc) — no schema conflict.
- Auth wording in `05`, `08`, `12` is tightened from "token or cookie/session" to this decision (Phase 4 patches).
- Refresh rotation + reuse detection is a Sprint 2 implementation and QA requirement (`13` high-risk area).

## References

- Decision 08 context (authentication was left open)
- ADR-0005 (Authorization and Identity Model)
- `05`, `07`, `08`, `12`, `13`
