# ADR-0009 — Public Content SEO Surface (Static Generation)

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Extends: ADR-0001 (Web Platform Strategy)
- Sources: Confirmed decision (2026-07-11); `05-system-architecture.md`, `09-ui-design-system.md`, `10-content-architecture.md`

---

## Context

ADR-0001 selected React Native Web with **no SSR** for the client application. That is the right call for the *application*, but it makes public content **client-side rendered**, which search engines index poorly.

This creates a true contradiction:

- `05` and `09` state SEO goals ("SEO where public content is available", "search engine indexing where allowed", "public shareable URLs").
- Organic search is the primary traffic channel for an educational content product.
- Developer Lab tools (pulled into MVP) depend almost entirely on search discovery ("jwt decoder", "cron expression builder").

Without an indexable surface, the content and the tools cannot be found.

## Decision

1. Add a **static generation (SSG) publishing target** that renders **public, read-only** content from `content/` into static HTML at build time.
2. **This is not a second application.** It is a third *renderer of the canonical content*, exactly like Knowledge Packs:

   ```
   content/**  (canonical Markdown + metadata — Single Source of Truth)
        ├─→ React Native app   (web + android + ios)   — interactive, authenticated
        ├─→ Knowledge Pack     (offline archive)
        └─→ Static HTML        (public, indexable)      ← this ADR
   ```

   ADR-0001 is unchanged: there is still **one** application, and it still has **no SSR**.

3. **SSG surface scope** (read-only, no authentication, no user state):
   - Topic pages
   - Roadmap pages
   - Terminology / glossary pages
   - Developer Lab tool pages (static shell + client-side tool logic)
   - `sitemap.xml`, `robots.txt`

   Everything interactive — progress, quiz, bookmarks, offline packs, settings, auth — remains **only** in the React Native app. Static pages offer a clear "continue in the app" entry point.

4. **SEO requirements** (implementation obligations):
   - **Internal linking generated from the Knowledge Graph** (ADR-0004): prerequisites / related / next rendered as descriptive anchor links. This is the primary topical-authority mechanism and the product's main SEO asset.
   - JSON-LD structured data: `Article` (topics), `BreadcrumbList`, `Course` (roadmaps), `FAQPage` (interview questions).
   - `hreflang` for `tr` / `en` (maps directly to the independent Content Language model).
   - Canonical URLs; stable slugs with a redirect strategy on change (per `05`).
   - URL structure per `05`: `/learn/{technology}/{version}/{topic-slug}`.
   - Core Web Vitals budget; static pages **must not** load the React Native app bundle.

5. **Constraints on the generator:**
   - Build-time only. **No new runtime framework** enters the application.
   - Consumes `content/` + design tokens from `packages/theme`.
   - Runs as a Turborepo task (ADR-0007).
   - Output is static HTML/CSS with minimal JS (tool pages excepted).
   - The concrete generator is an implementation detail chosen in Sprint 1 under these constraints.

6. **Sequencing:** the build target is scaffolded in Sprint 1; it becomes meaningful when content lands (Sprint 3+).

## Alternatives Considered

- **SSR inside the RN-Web app (Next.js).** Rejected: contradicts ADR-0001, adds a runtime framework, and ships the app bundle to readers who only want to read.
- **A second React/Next.js web application.** Rejected: duplicate UI, duplicate maintenance — explicitly not wanted.
- **Accept no SEO.** Rejected: incompatible with the stated traffic goal and makes Developer Lab's MVP inclusion largely pointless.

## Consequences

- SEO statements in `05` and `09` become implementable instead of aspirational (their earlier "post-MVP" classification in ADR-0001 is lifted for this surface).
- Two independent pipelines: **app releases** (RN/store/web deploy) and **content publishing** (static regeneration on content merge). Neither breaks the other.
- Adds one build-time dependency and one deploy target (static hosting + CDN). Runtime complexity of the app is unchanged.
- `06-monorepo-structure.md` gains this build target (Phase 4 patch).

## References

- ADR-0001 (Web Platform Strategy) — extended, not superseded
- ADR-0004 (Knowledge Graph), ADR-0007 (Monorepo Tooling)
- `05`, `09`, `10`
