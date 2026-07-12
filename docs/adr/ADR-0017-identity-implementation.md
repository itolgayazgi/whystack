# ADR-0017 — Identity Implementation

- Status: Accepted
- Date: 2026-07-12
- Owner: WhyStack Core Team
- Extends: ADR-0005 (Authorization and Identity Model), ADR-0008 (Authentication Strategy)
- Sources: Confirmed decision (2026-07-12); `07-database-design.md`, `08-api-standards.md`, `12`, `13`

---

## Context

ADR-0005 fixed *who* the roles are. ADR-0008 fixed *how* identity is proven — short-lived JWT access
token plus a rotating refresh token with reuse detection. Neither says *what builds it*.

`07-database-design.md` left that open, explicitly:

> "The final implementation may use ASP.NET Core Identity or a custom identity model. If ASP.NET Core
> Identity is used, default tables may be customized or wrapped by WhyStack domain tables."

Sprint 2 cannot start without closing it. The choice shapes the schema, the layer boundaries, and the
size of the surface we have to secure ourselves.

## Decision

**A custom identity model, built on ASP.NET Core Identity's cryptographic primitives.**

Concretely:

1. **Entities are ours.** `Users`, `Roles`, `UserRoles`, `UserSessions`, `UserLoginEvents`,
   `PasswordResetTokens`, `EmailConfirmationTokens`, `UserPreferences` — exactly as `07` names them.
   They live in `WhyStack.Domain`, which depends on nothing.

2. **The cryptography is not ours.** We take the vetted pieces and nothing else:
   - `Microsoft.AspNetCore.Identity.PasswordHasher<T>` for password hashing and verification.
     It is PBKDF2-HMAC-SHA512, 210,000 iterations, per-password salt, and it upgrades a hash to the
     current parameters on successful sign-in. **We do not write password hashing.**
   - `RandomNumberGenerator` for token material, and SHA-256 for the stored hash of a refresh,
     reset or confirmation token — never the raw token (ADR-0008, `07`).
   - ASP.NET Core's JWT bearer handler for validating access tokens.

3. **`UserManager`, `SignInManager`, `IdentityDbContext` and the `AspNet*` tables are NOT used.**

## Why

**ASP.NET Core Identity's schema is not our schema.** It brings `AspNetUsers`, `AspNetRoles`,
`AspNetUserTokens` and friends. `07` specifies plural WhyStack tables with `Id` primary keys,
`{Entity}Id` foreign keys, `...Utc` timestamps and `Is...` booleans. Reconciling the two means
customising every table and wrapping every manager — real work, producing a model that is neither
theirs nor ours, and harder to reason about than either.

**It does not give us the part that is actually hard.** ADR-0008 requires a refresh token that is
**rotated on every use, with reuse detection that revokes the whole session family**. ASP.NET Core
Identity has no such concept. We are writing that regardless. Adopting the full framework buys us
password hashing and lockout — and leaves the highest-risk mechanism entirely to us anyway.

**Layer boundaries stay clean.** `UserManager<TUser>` depends on EF Core and on the DI container.
Using it in the Application layer would drag both across a boundary that CLAUDE.md §3 and `12` call a
blocking defect. A `User` entity with no dependencies does not.

## What this costs — stated plainly

We take on the parts Identity would have handed us:

| Piece | Our obligation |
|---|---|
| Account lockout after repeated failures | Implement and test it. `UserLoginEvents` already exists to drive it. |
| Email confirmation and password reset tokens | Generate from a CSPRNG, store only the hash, expire them, single-use. |
| Email normalisation and uniqueness | `NormalizedEmail` with a unique index — the schema in `07` already says so. |
| Security stamp / global invalidation | Session revocation via `UserSessions` covers it; there is no separate stamp. |

**Not writing our own password hashing is what makes this acceptable.** That is where hand-rolled
auth actually gets people, and we are not doing it. Everything else on that list is application logic
with a testable definition of correct — and `13` marks it high-risk, so it will be tested as such:
reuse detection, enumeration, lockout, token expiry and single-use are QA requirements, not optional
extras.

## Alternatives Considered

- **Full ASP.NET Core Identity.** Rejected on schema conflict and layer leakage, as above. It would be
  the right call if we needed its breadth — external providers, 2FA, claims-heavy authorisation. We do
  not: ADR-0005 uses plain role checks in the MVP, and ADR-0008 defines a single credential flow.
- **A third-party identity provider (Auth0, Entra ID, Keycloak).** Rejected for the MVP: it moves user
  data off-platform, adds a runtime dependency and a cost, and the offline-first requirement means we
  own session state regardless. Revisit if enterprise SSO ever becomes a requirement (`01` names B2B as
  a future path).
- **Hand-rolled hashing (bcrypt/argon2 via a library).** Rejected: `PasswordHasher<T>` is already
  present, already vetted, and already handles parameter upgrades on sign-in. Choosing a different KDF
  would be a preference, not an improvement.

## Consequences

- `07`'s open question is closed. Its Identity tables are implemented as written.
- `Microsoft.AspNetCore.Identity` is referenced from **Infrastructure only** — for `PasswordHasher<T>`.
  Neither `Domain` nor `Application` sees it. Application depends on an `IPasswordHasher` port we own.
- Lockout, token expiry, single-use tokens and reuse detection become explicit, tested requirements
  rather than inherited behaviour. `13` already classifies them as high-risk.
- If enterprise SSO is ever needed, this model does not block it: an external provider becomes another
  way to populate `Users`, not a rewrite.

## References

- ADR-0005 (Authorization and Identity Model), ADR-0008 (Authentication Strategy)
- `07` (Identity Domain — schema), `08` (auth endpoints), `12` (logging rules), `13` (high-risk areas)
