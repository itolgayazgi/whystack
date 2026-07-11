# ADR-0001 — Web Platform Strategy

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Supersedes: —
- Extended by: **ADR-0009 (Public Content SEO Surface)** — the SEO limitation described below is resolved by a static publishing target, not by adding SSR to this application.
- Sources: Decision 03; `05-system-architecture.md`, `06-monorepo-structure.md`, `09-ui-design-system.md`

---

## Context

The Foundation Pack was ambiguous about how the Web application is built:

- `05-system-architecture.md` states React Native shares client logic and supports a "Web target where applicable."
- `06-monorepo-structure.md` defines two separate app folders (`apps/web/` and `apps/mobile/`) with different internal structures, implying two distinct client applications.
- `09-ui-design-system.md` requires web-specific layouts ("Web should not simply stretch the mobile UI").

This ambiguity blocked Sprint 1, which must scaffold Web, Android and iOS targets. A separate React/Next.js web app would duplicate UI, violate **Cross-Platform Equality** (Principle 31) and **One Design System**, and increase maintenance for a solo-founder MVP.

## Decision

1. The Web application is built with **React Native Web**.
2. There is **one** React Native client application. Web is a **render target** of that application, alongside Android and iOS. There is **no** separate React web application.
3. **No Next.js** and **no SSR/SSG** in Sprint 1. The Web target is client-side rendered.
4. Web-specific *layout adaptation* (reading canvas, side navigation, table of contents) is achieved through the responsive rules in `09-ui-design-system.md`, not through a separate stack.
5. Future migration to SSR/SSG for public pages remains possible via a future ADR; it is out of MVP scope.

## Alternatives Considered

- **Separate React + Next.js web app.** Rejected: duplicates the entire UI layer, breaks single-design-system, doubles QA surface, and contradicts MVP-first.
- **RN-Web now, SSR in Sprint 1.** Rejected: SSR adds infrastructure and build complexity not justified by MVP scope.

## Consequences

- **Positive:** One codebase and one design system across all platforms; lowest maintenance; consistent product meaning.
- **Negative — SEO:** Client-side-only rendering ranks poorly in search engines. This **conflicts with the SEO statements** in `05-system-architecture.md` and `09-ui-design-system.md`. Those statements are hereby classified **post-MVP**. If organic search traffic becomes a business priority, public topic pages will require a future **SSR/SSG ADR** — SEO cannot be treated as a client-only feature.
- **Monorepo impact:** `06-monorepo-structure.md`'s separate `apps/web/` + `apps/mobile/` no longer reflects reality. A documentation patch consolidates them into a single React Native client application (see ADR-0007 and Phase 4 patches).

## References

- Decision 03 (Web Architecture)
- ADR-0007 (Monorepo Tooling)
- `05`, `06`, `09`
