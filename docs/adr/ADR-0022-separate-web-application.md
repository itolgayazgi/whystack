# ADR-0022 — A Separate Web Application

- Status: Accepted
- **Supersedes: ADR-0001** (Web Platform Strategy)
- **Supersedes: ADR-0009** (Public Content SEO Surface) — the *surface* survives; the *application* that produces it changes
- Date: 2026-07-14
- Owner: WhyStack Core Team
- Sources: `01-product-vision.md`; `02-product-principles.md` (Principle 31); `06-monorepo-structure.md`; `09-ui-design-system.md`; ADR-0001, ADR-0009, ADR-0016, ADR-0020, ADR-0021

---

## Context

ADR-0001 decided there would be **one** client application — React Native, with web as a render target — and
explicitly rejected a separate React web app on the grounds that it "duplicates the entire UI layer".

That was the right call for what the product was then: a reading app.

It is the wrong call for what the product is now, and the evidence arrived from three directions at once:

**1. The web is where content gets AUTHORED.** ADR-0020 moved the source of truth into the database and made
the application the authoring surface. A content studio is a desktop tool — a Markdown editor with live
preview, section reordering, a relationship picker, keyboard shortcuts. Building that in React Native Web
means fighting the platform for every one of those, and the result would be a worse tool than the one the
editor deserves.

**2. The web already had TWO half-surfaces, and neither was whole.**

| | what it is | what it cannot do |
|---|---|---|
| `apps/site` (Astro) | static, indexable, zero JS | no session — nobody can sign in |
| `apps/client` web target | the app, in a browser | renders no HTML — a crawler sees a spinner |

A reader who lands on the marketing page and presses "sign in" is handed to a different application. That is
not a product; it is two halves of one wearing the same colours.

**3. Nothing is being ADDED.** This is the part that decides it. Replacing two half-surfaces with one whole
one leaves the surface count where it was:

```
BEFORE                              AFTER
apps/api                            apps/api
apps/client   web + android + ios   apps/client   android + ios      ← a reading device
apps/site     static, no session    apps/web      the whole website
```

## Decision

**1. `apps/web` is a Next.js application, and it is the website.** Landing, sign-in, registration, reading,
progress — and the **content studio**. One codebase, one origin, one thing a person can call "the site".

**2. `apps/client` drops its web target.** It is Android and iOS: a focused reading and progress client. That
is not a demotion — it is what a phone is *for* in this product. Reading on a train is the job it does better
than a laptop ever will.

**3. `apps/site` is folded into `apps/web` and deleted.** Next.js static generation produces the public,
indexable pages ADR-0009 requires. What ADR-0009 *decided* — that public content must be crawlable HTML,
served without an application bundle — is unchanged and is now delivered by the same app the reader signs in
to.

**4. What is shared is shared, and what is duplicated is named.**

Shared, and it is most of the hard part:

- The **API contract**, extracted to `packages/api-client` (`06` already approves this package).
- **`packages/theme`** — every colour, size and space. One design system, two renderers.
- **`packages/markdown-renderer`** — the *parser* is already a separate entry point from the React Native
  renderer, precisely so a second renderer could exist. A DOM renderer is a third entry point over the same
  tree.

Duplicated, honestly:

- Auth screens, settings, and the reading screen. Roughly six screens today.

**5. The duplication is bounded because the products diverge.** Mobile *reads*. Web reads **and authors and
manages**. A content studio, a roadmap editor and an analytics view will never exist on the phone, and a
reading screen tuned for a train will never be the one a laptop shows. They are not two copies of one app;
they are two apps for two jobs.

## Cross-Platform Equality is not broken

`02` Principle 31 is explicit, and it is about **educational quality**:

> "WhyStack should provide equivalent educational quality across Web, Android, iOS. Platform-specific
> optimizations are encouraged. **Educational inequality is not.**"

Authoring is not education. A content studio on web only does not make a phone reader learn less. **Reading
stays equal on all three**, and `13` Objective 03's platform-drift check applies to reading exactly as it did
before.

## Alternatives Considered

- **Keep one React Native codebase (ADR-0001).** Rejected now, correctly accepted then. The deciding factor
  is the studio: it is a desktop tool, it is needed urgently, and RN Web is the wrong material for it.

- **Build the studio as a third app (`apps/admin`).** `06` reserves that folder. Rejected: it would put
  authoring behind a *different login on a different origin*, and the person authoring is also the person
  reading. One website, one session, one place.

- **Keep Astro for the landing and add auth to it.** Rejected: adding a session to a zero-JS static site
  means adding JavaScript, state and a bundle — which is to say, building `apps/web` badly.

## Consequences

- **ADR-0001 is superseded.** Its Decisions 1–3 are reversed. Its *reasoning* — that duplicating a UI layer is
  expensive — stands, and is why Decision 4 above exists.
- **ADR-0009 is superseded as an application decision.** Its *requirement* — public content must be indexable
  HTML that ships no application bundle — is inherited by `apps/web`'s static pages, and the build check that
  measures it moves with it.
- `06` § Web Application Structure is patched: `apps/web` is Next.js; `apps/site` is removed.
- `packages/api-client` is created (`06` already approves it) and both clients consume it.
- `packages/markdown-renderer` gains a `/web` entry point over the same parse tree.
- The migration is incremental: `apps/site` stays alive until `apps/web` serves the landing page, so the
  public surface is never dark.

## References

- ADR-0001 (superseded), ADR-0009 (superseded), ADR-0016 (React Native — unchanged for mobile)
- ADR-0020 (database is the source of truth — the reason the studio exists), ADR-0021
- `02` Principle 31 · `06` · `09` · `13` Objective 03

---

## Amendment — 2026-07-16: the studio is web-only

**Status:** Accepted · **Decider:** Tolga Yazgı (owner)

> *mobil tarafta Studio diye bir alanımız olmayacak. O kısım sadece Web'e özel.*

The mobile app has no studio: no route, no `canAuthor` check, no tab. This is already true — the amendment
is here so it stays true rather than surviving on nobody having thought about it.

**Why it is the right shape, not just the current one.** Authoring is a keyboard job on a wide screen. The
studio's own design is three columns at 1360px — a queue, a block editor and a publish panel — and there is
no honest reduction of that to 375px. A phone-shaped studio would be a worse studio and a distraction in an
app whose whole surface is `Bugün / Hattım / Keşfet / Profil`.

**What this means in practice:**

- `apps/client/src/config/product-areas.ts` stays at four reader areas. A fifth entry for authoring is the
  thing this amendment forbids.
- The mobile app does not need to know what an editor is. `canAuthor` and the role names belong to
  `apps/web`; importing them into the client would be the seam leaking.
- The server does not change. Roles and the authoring endpoints are enforced server-side regardless of who
  is asking (CLAUDE.md §6) — this is about which surface offers the door, never about who may open it.
