# ADR-0005 — Authorization and Identity Model

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Sources: Decision 08 (+ confirmed Sprint 1 scope); `05-system-architecture.md`, `07-database-design.md`, `08-api-standards.md`, `13-quality-assurance.md`, `14-agent-ecosystem.md`

---

## Context

Decision 08 revised the role set, but the old four-role model (`Learner, ContentReviewer, Editor, Administrator`) is hardcoded across `05`, `07` (Roles seed), `08`, `13` (authorization matrix) and `14`. This must be reconciled to a single canonical model. Authorization (what a role may do) is defined here; the authentication mechanism (how identity is proven) is in ADR-0008.

## Decision

1. **Canonical roles (role-based authorization):**

   `Guest, RegisteredUser, PremiumUser, Editor, Reviewer, Translator, Administrator`

2. **Name migration** (old → canonical): `Anonymous → Guest`, `Learner → RegisteredUser`, `ContentReviewer → Reviewer`. `Editor` and `Administrator` are unchanged.
3. **Sprint 1 scope (confirmed):**
   - **Active in Sprint 1:** `Guest`, `RegisteredUser`, `Administrator`.
   - **Seeded but dormant** (defined in the `Roles` table; behaviors bound in later sprints): `PremiumUser`, `Editor`, `Reviewer`, `Translator`.
4. **`PremiumUser`** reserves the tier for future monetization. **No billing, entitlements, or paywall logic** ship in Sprint 1 — it is a role placeholder only. Monetization design remains deferred (Principles 28–30).
5. **Fine-grained / policy-based permissions** (e.g. `CanPublishContent`, `CanManageUsers`) are introduced **after Sprint 1** via a future ADR. Sprint 1 uses role checks only.
6. **Ownership checks** (user may act only on their own progress/bookmarks/preferences) apply in Sprint 1 independent of role, per `12`/`13`.

## Alternatives Considered

- **Keep the four-role model.** Rejected: contradicts Decision 08.
- **Implement all seven roles fully in Sprint 1.** Rejected: editorial and premium flows don't exist yet — premature complexity, violates MVP-first.
- **Guest + RegisteredUser only (no Admin).** Rejected: content/role administration needs `Administrator` from day one.

## Consequences

- `07` `Roles` seed updated to the seven canonical roles (Phase 4 patch).
- `13` authorization matrix updated to `Guest / RegisteredUser / PremiumUser / Editor / Reviewer / Translator / Administrator / ResourceOwner / NonOwner` (Phase 4 patch).
- `05`, `08`, `14` role references updated to canonical names (Phase 4 patches).
- Dormant roles exist in data but grant no Sprint-1 capabilities beyond `RegisteredUser` until wired.

## References

- Decision 08
- ADR-0008 (Authentication Strategy)
- `05`, `07`, `08`, `13`, `14`
