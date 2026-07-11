# WhyStack Site

The public, indexable static surface (ADR-0009).

**This is not a second application.** It is a *third renderer of the canonical content*, exactly like a
Knowledge Pack:

```text
content/**  (canonical Markdown + metadata — Single Source of Truth)
     ├─→ React Native app   (web + android + ios)   — interactive, authenticated
     ├─→ Knowledge Pack     (offline archive)
     └─→ Static HTML        (public, indexable)      ← this
```

ADR-0001 is unchanged: there is still **one** application, and it still has **no SSR**.

## Run

```bash
pnpm --filter @whystack/site dev      # http://localhost:4321
pnpm --filter @whystack/site build    # builds, then verifies its own output
```

## Why Astro

Chosen in Sprint 1 under the constraints ADR-0009 set. The decisive one: **it ships zero JavaScript
unless a page explicitly asks for it.** ADR-0009 says static pages must not load the React Native app
bundle — a reader who came to read must not download an application — and commits this surface to a
Core Web Vitals budget.

When Developer Lab tool pages arrive (the one exception ADR-0009 carves out), Astro's islands put JS on
*that page only*, not on every topic.

## The build verifies itself

`scripts/verify-static-output.mjs` runs as part of `build`, not as a step someone can forget. It fails
the build if:

- **any page ships a `<script>`** — except JSON-LD, which executes nothing and is required by ADR-0009.
  A route that genuinely needs JS goes in `ALLOWED_JS_ROUTES`, one route at a time, in a diff a human
  reads.
- **`robots.txt`, `sitemap-index.xml` or `llms.txt` is missing.** These are *generated artifacts*
  (ADR-0011). If the build quietly stopped emitting one, the site would go on looking fine while
  telling crawlers nothing.

A framework that ships zero JS today ships some the first time somebody adds a component that needs it.
Nobody notices until a Lighthouse run months later. So it is measured.

## robots.txt is generated, not written

ADR-0011: *"robots.txt is a generated artifact of the SSG build, not a hand-edited file."*

The policy lives in `src/pages/robots.txt.ts`, and the file is derived from it. **All well-behaved
crawlers are allowed, training crawlers included** — `GPTBot`, `CCBot`, `Google-Extended`.

That was a deliberate reversal. Blocking them is right for an established publisher with negotiating
leverage. WhyStack has none: no brand, no revenue, free content, and a primary problem of obscurity
rather than theft. The moat is not the prose — it is the curation, the sequence, the roadmaps, the
offline packs and the tools, and none of those can be trained into a model.

## Design tokens

Read from `@whystack/theme` and projected into CSS custom properties in `src/styles/tokens.ts`. **No hex
literal appears in this app.** A copied colour is a second source of truth, and the second one always
wins quietly — you change the app, the site keeps the old blue, and nobody notices for a month.

## Not done yet

**The fonts of ADR-0013 are not loaded.** Doing it properly means subsetting Literata, Inter and
JetBrains Mono to Latin + Latin Extended-A, self-hosting `woff2`, and preloading only the body face.
Loading them from a CDN instead would be one line and would quietly spend the Core Web Vitals budget
this ADR commits to. Until the files exist, the fallback stacks render. This is written down so nobody
mistakes it for finished.

**There are no topic pages**, because there are no topics. Content lands in Sprint 3. Generating
placeholder topics would produce exactly the thin pages that search engines punish.
