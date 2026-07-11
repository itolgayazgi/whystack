# ADR-0013 — Typography Stack

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Sources: Confirmed delegation (2026-07-11); `03-philosophy.md`, `09-ui-design-system.md`, ADR-0009
- Implemented by: `docs/design-system/design-tokens.md`, `packages/theme`

---

## Context

`09-ui-design-system.md` defines typography **rules** (hierarchy, reading comfort, line-height range, code requirements) but explicitly leaves the values open: *"The exact font family may be selected during implementation."* `packages/theme` is therefore an empty shell, and the gap between "correct rules" and "a product people call beautiful" is exactly these values.

`03-philosophy.md` states the product is *"a reading platform before it becomes an interaction platform."* Typography is therefore not decoration — it is the primary interface.

Four constraints bound the choice:

1. **Turkish diacritics are mandatory** — `ı İ ğ Ğ ş Ş ç Ç ö Ö ü Ü`. Many editorial serifs render the dotless `ı` and dotted `İ` poorly or not at all. This is non-negotiable for a Turkish/English product.
2. **Licensing** — commercial editorial serifs require separate web, iOS and Android embedding licences. With no MVP revenue (ADR-0012), this cost is not justifiable.
3. **Performance** — ADR-0009 commits the public surface to Core Web Vitals. Font files directly affect LCP.
4. **Technical content** — the product is not pure prose. It contains code, tables, metadata and badges. A single family cannot serve all three roles well.

## Decision

A **three-role type system**, all families OFL-licensed (free to embed on web, iOS and Android), all **variable** (one file, all weights), all with complete Turkish coverage.

| Role | Family | Rationale |
|---|---|---|
| **Body prose** | **Literata** | Designed by TypeTogether for **Google Play Books** — purpose-built for long-form on-screen reading. Delivers the editorial reading feel that motivated this decision. Variable, OFL, full Turkish support. |
| **UI / headings** | **Inter** | The de-facto interface typeface. Exceptional legibility at small sizes (labels, metadata, badges, navigation). Variable, OFL, flawless Turkish. |
| **Code** | **JetBrains Mono** | Purpose-designed for code, favoured by the .NET/developer audience. Clear character disambiguation. Variable, OFL. |

### Headings use the sans (Inter), not the serif

A deliberate functional split:

- **Body = prose → read linearly.** Serif maximises long-form comfort.
- **Headings = structure → scanned, deep-linked, listed in the table of contents.** A WhyStack topic carries 20–30 sections (`10` blueprint). Sans headings scan faster, form a clearer hierarchy at that density, and visually bind to the ToC and UI chrome.

A fully-serif page reads more like an essay; WhyStack topics are structured references that are *both* read and navigated.

### Drop caps are not adopted

Attractive in essays, but WhyStack topics are scanned and deep-linked (`#why-it-exists`). Under `09`'s own test — *"Does this help learning?"* — a drop cap does not qualify, and React Native has no `::first-letter` equivalent.

### Loading strategy

- Subset to **Latin + Latin Extended-A** (this range covers all Turkish diacritics).
- `woff2` on web; **preload the body font only** (Literata), `font-display: swap` for the rest.
- Bundle `.ttf` in the React Native app — no runtime font fetch on mobile.
- Static SEO pages (ADR-0009) must not regress LCP through font weight.

## Alternatives Considered

- **Commercial editorial serif** (Tiempos, Lyon, Freight, Publico). Rejected: multi-platform embedding licence cost with zero MVP revenue; Turkish coverage is inconsistent and must be verified per family.
- **Source Serif 4 + Source Sans 3 + Source Code Pro.** A valid alternative — one coherent superfamily, simpler to maintain. Rejected only because Literata is measurably better tuned for long-form screen reading, which is this product's core activity. Retained as the fallback if Literata causes issues.
- **Serif headings (fully editorial).** Rejected: at 20–30 sections per topic, scannability outweighs unity.

## Consequences

- `packages/theme` is founded on these three families.
- Concrete values (scale, line-heights, colours, spacing, motion) are specified in **`docs/design-system/design-tokens.md`**, which `09` owns and references. `09` owns the *rules*; the token document owns the *values*.
- **A `09` correction is required:** `09` specifies a desktop reading column of `720px–860px`. At the chosen body size that yields ~85–95 characters per line — beyond the comfortable 65–75 range. The measure is redefined in character units (`68ch`), which is font-size independent and more robust. This is a Phase 4 patch to `09`.

## References

- `docs/design-system/design-tokens.md`
- ADR-0009 (Core Web Vitals commitment), ADR-0012 (no revenue → licence cost matters)
- `03`, `09`
