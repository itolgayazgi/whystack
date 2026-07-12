# WhyStack — Design Tokens

- Version: 1.1.0
- Status: Colour **verified** against WCAG AA (2026-07-11). Typography rendering still pending verification.
- Owner: WhyStack Core Team
- Parent: **`09-ui-design-system.md`** — that document owns the **rules**; this document owns the **values**.
- Decided by: ADR-0013 (Typography Stack)
- Implemented in: `packages/theme`

> This is the concrete token specification. Applications and shared components **must** consume these tokens. Hardcoded colours, sizes, spacing, radii or durations are forbidden (`09` Claude Code Rule 03).

---

## 1. Typography

### Families

| Token | Family | Fallback stack |
|---|---|---|
| `font.body` | **Literata** (variable) | `Georgia, 'Times New Roman', serif` |
| `font.ui` | **Inter** (variable) | `system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif` |
| `font.code` | **JetBrains Mono** (variable) | `ui-monospace, 'SF Mono', Consolas, monospace` |

All three are OFL-licensed, variable, and cover **Latin + Latin Extended-A** (all Turkish diacritics).

**Verification test string** — every font must render this correctly before acceptance:

```
Iğdır'da sığınmış çilingir, İzmir'de öğün. — ı İ ğ Ğ ş Ş ç Ç ö Ö ü Ü
```

### Scale

Sizes are density-independent (React Native) / `px` (web). Two columns: **Compact** (phones) and **Expanded** (tablet/desktop). Medium interpolates; Wide uses Expanded.

| Token | Family | Compact | Expanded | Line height | Weight | Use |
|---|---|---|---|---|---|---|
| `text.display` | ui | 32 | 44 | 1.15 | 700 | Landing / hero only. Never inside a topic. |
| `text.pageTitle` | ui | 28 | 34 | 1.25 | 700 | Topic title, roadmap title |
| `text.sectionTitle` | ui | 22 | 24 | 1.35 | 600 | Blueprint sections (Definition, Why It Exists…) |
| `text.subsectionTitle` | ui | 18 | 19 | 1.40 | 600 | Divisions inside a section |
| `text.bodyLarge` | **body** | 19 | 20 | 1.70 | 400 | Topic intro, summary, learning objectives |
| `text.body` | **body** | 18 | 19 | **1.75** | 400 | **Primary reading style.** The most important token. |
| `text.bodySmall` | **body** | 16 | 16 | 1.65 | 400 | Secondary explanation |
| `text.caption` | ui | 13 | 13 | 1.45 | 400 | Image/diagram captions, metadata notes |
| `text.label` | ui | 13 | 13 | 1.30 | 500 | Badges, tags, form labels, nav labels |
| `text.code` | code | 14.5 | 15 | 1.60 | 400 | Code blocks |
| `text.codeInline` | code | 0.92em of parent | — | inherit | 400 | Inline code inside prose |

**Headings use the sans (Inter) by design** — topics carry 20–30 sections and headings are scanned and deep-linked, not read linearly (ADR-0013).

### Reading measure

| Token | Value | Note |
|---|---|---|
| `reading.measure` | **68ch** | ≈ 65–75 characters — the comfortable range. Expressed in characters, not px, so it stays correct if the body size changes. |
| `reading.maxWidth` | `min(68ch, 100% - 2 × gutter)` | Resolves to ~660–700px at the expanded body size. |

> **Correction to `09`:** `09` currently states a `720px–860px` reading column. At the chosen body size that produces ~85–95 characters per line — too long; the eye loses the line start. The character-based measure above supersedes it (Phase 4 patch).

### Paragraph rhythm

| Token | Value |
|---|---|
| `reading.paragraphSpacing` | `space.20` (20) |
| `reading.sectionSpacing` | `space.48` (48) |
| `reading.headingTopSpacing` | `space.40` (40) |
| `reading.headingBottomSpacing` | `space.12` (12) |

Headings sit **close to the paragraph below** and far from the one above — this is what makes structure legible without borders.

### Reading font scale

`04` requires "Reading settings" and `07` carries a `ReadingFontScale` column, but neither document gave it a range. This defines it.

| Step | Value | Body size (compact → expanded) |
|---|---|---|
| Small | `0.875` | 15.75 → 16.6 |
| **Default** | **`1.0`** | 18 → 19 |
| Large | `1.25` | 22.5 → 23.75 |
| Largest | `1.5` | 27 → 28.5 |

**Discrete steps, not a slider.** Type is a scale, not a number: size, line height and the 4px vertical rhythm are chosen together, so an arbitrary `1.0732×` puts the baseline between grid lines on every screen in the product. Four steps are four layouts that can be tested. A continuous range is infinitely many that cannot.

The floor is `0.875` and not lower because the body token is already the smallest comfortable long-form size; the ceiling is `1.5` because past it the reading measure collapses below ~45 characters on a phone and the line-start becomes hard to find — the same failure the character-based measure above exists to prevent.

This **does not replace the operating system's text-size setting**, which the client honours regardless. It is for the reader who wants long-form prose larger without enlarging every other app they own.

> **Single source:** `packages/theme/src/reading-font-scale.json`. The API validates against the same numbers in C#, and a test on each side reads that file — so the two cannot drift apart silently.

---

## 2. Spacing

Base unit 4. Scale as defined in `09`:

```
space.0=0   space.2=2   space.4=4   space.8=8   space.12=12
space.16=16 space.20=20 space.24=24 space.32=32 space.40=40
space.48=48 space.64=64 space.80=80 space.96=96
```

| Token | Value | Use |
|---|---|---|
| `gutter.compact` | 20 | Page side padding, phones |
| `gutter.expanded` | 32 | Page side padding, tablet |
| `gutter.wide` | 48 | Page side padding, desktop |

---

## 3. Colour

**Design intent:** a warm, low-glare paper feel in light mode; a soft, non-black dark mode. No pure `#000` or `#FFF` text — both cause halation with a serif body.

### Light

| Token | Value |
|---|---|
| `background` | `#FCFBF9` (warm paper) |
| `surface` | `#FFFFFF` |
| `surfaceElevated` | `#FFFFFF` (+ `elevation.1`) |
| `surfaceMuted` | `#F4F2EF` |
| `textPrimary` | `#1A1A18` |
| `textSecondary` | `#55534E` |
| `textMuted` | `#716E69` |
| `border` | `#E6E3DE` |
| `borderStrong` | `#CFCBC4` (decorative) |
| `borderInteractive` | `#716E69` — **the boundary of a control**: inputs, checkboxes, outlined buttons |
| `accent` | `#1D5D8C` |
| `success` | `#2E7C5A` |
| `warning` | `#96651A` |
| `error` | `#B3423C` |
| `info` | `#2D6A9F` |
| `ai` | `#6A4FA3` |
| `deprecated` | `#876A3A` |
| `offline` | `#4A6B5C` |
| `codeBackground` | `#F5F2EE` |
| `codeText` | `#1A1A18` |
| `focusRing` | `#1D5D8C` |

### Dark

| Token | Value |
|---|---|
| `background` | `#16171A` (never pure black) |
| `surface` | `#1D1E22` |
| `surfaceElevated` | `#24252A` (elevation by lightness, not shadow) |
| `surfaceMuted` | `#202126` |
| `textPrimary` | `#E8E6E1` (warm off-white) |
| `textSecondary` | `#A8A59E` |
| `textMuted` | `#8E8B85` |
| `border` | `#2E2F35` |
| `borderStrong` | `#43444B` (decorative) |
| `borderInteractive` | `#8E8B85` — **the boundary of a control** |
| `accent` | `#6FA8D6` |
| `success` | `#5FB88C` |
| `warning` | `#D9A441` |
| `error` | `#E0736C` |
| `info` | `#6FA8D6` |
| `ai` | `#A98FD9` |
| `deprecated` | `#C4A063` |
| `offline` | `#7FA893` |
| `codeBackground` | `#1D1E22` |
| `codeText` | `#E8E6E1` |
| `focusRing` | `#6FA8D6` |

### Contrast audit (2026-07-11) — **verified**

Every text-bearing token was measured against **every surface it can land on**, in both schemes
(WCAG 2.1 AA: 4.5:1 text, 3:1 non-text). Five tokens failed and were adjusted by changing lightness
only — hue and saturation are unchanged, so the designed warmth is preserved. The worst-case
background is `surfaceMuted` in light and `surfaceElevated` in dark.

| Token | Was | Now | Worst ratio before | Worst ratio after |
|---|---|---|---|---|
| `light.textMuted` | `#8A8781` | **`#716E69`** | 3.21:1 ❌ | 4.54:1 ✅ |
| `light.warning` | `#B0761E` | **`#96651A`** | 3.44:1 ❌ | 4.50:1 ✅ |
| `light.success` | `#2E7D5B` | **`#2E7C5A`** | 4.47:1 ❌ | 4.53:1 ✅ |
| `light.deprecated` | `#8A6D3B` | **`#876A3A`** | 4.34:1 ❌ | 4.53:1 ✅ |
| `dark.textMuted` | `#75726C` | **`#8E8B85`** | 3.19:1 ❌ | 4.50:1 ✅ |

`success` and `deprecated` were missed by a first, hand-listed audit that only checked them against
`background` and `surface`. They fail only on `surfaceMuted` — a card on a muted panel. The audit is
therefore **combinatorial, not hand-listed**: `packages/theme/src/contrast.test.ts` enumerates every
text token × every surface, so no pair can be forgotten again.

`border` (1.24:1) and `borderStrong` (1.56:1) do **not** meet 3:1 and are **knowingly left as they are**.
WCAG 1.4.11 applies to visual information *required to identify* a component or state; a decorative
separator between blocks of already-legible content is exempt. Raising them to 3:1 would produce hard
grey rules and destroy the paper feel `09` asks for.

> **This obligation is now discharged (2026-07-12).** The moment a control's boundary is the *only* thing
> identifying it — a text input, a checkbox, an unfilled button — `border` and `borderStrong` may **not** be used,
> because both fail 3:1 (they reach 1.45:1 and 1.58:1). Use **`borderInteractive`**, which is audited at
> ≥ 3:1 against every surface in `contrast.test.ts`.
>
> **Text on a filled `accent` button** uses `background` (6.78:1 light, 7.04:1 dark) and is audited too.
> `textPrimary` on `accent` is 2.49:1 and would be illegible — the two are different questions with
> different answers, and only one of them was covered by the original audit.

### Colour rules (from `09`, restated as obligations)

- **Contrast is verified, not assumed.** Any token change re-runs the audit above. A failing pair is adjusted, never shipped.
- **Colour is never the only signal.** Deprecated, AI-generated, offline, error and success states must each carry a text label and/or icon (`09` Forbidden Pattern 06).
- `ai` (violet) is deliberately unlike every other semantic colour — AI-generated content must never be mistakable for official content.

---

## 4. Radius

| Token | Value | Use |
|---|---|---|
| `radius.small` | 6 | Inputs, small badges |
| `radius.medium` | 10 | Cards, callouts, code blocks |
| `radius.large` | 16 | Large panels, sheets |
| `radius.full` | 9999 | Pills, tags, avatars |

---

## 5. Elevation

Subtle only. In **dark mode, elevation is expressed through surface lightness, not shadow.**

| Token | Light |
|---|---|
| `elevation.0` | none |
| `elevation.1` | `0 1px 2px rgba(20,18,15,0.06)` |
| `elevation.2` | `0 2px 8px rgba(20,18,15,0.08)` |
| `elevation.3` | `0 8px 24px rgba(20,18,15,0.10)` |

Never use elevation as decoration (`09`).

---

## 6. Motion

| Token | Value |
|---|---|
| `duration.fast` | 120ms |
| `duration.base` | 200ms |
| `duration.slow` | 320ms |
| `easing.standard` | `cubic-bezier(0.2, 0, 0, 1)` |
| `easing.exit` | `cubic-bezier(0.4, 0, 1, 1)` |

**Reduced motion:** when enabled, all durations collapse to `0ms` except opacity fades, which may use `duration.fast`. Learning must remain fully functional (`09`).

---

## 7. Breakpoints

| Token | Range | Layout mode |
|---|---|---|
| `breakpoint.compact` | `< 600` | Single column, bottom nav, collapsed ToC |
| `breakpoint.medium` | `600 – 904` | Wider measure, collapsible side nav |
| `breakpoint.expanded` | `905 – 1239` | Main column + side navigation + ToC |
| `breakpoint.wide` | `≥ 1240` | As expanded; **measure does not grow** — extra space becomes gutters and panels |

---

## 8. Reading screen restraint rule

> **At most 3 secondary actions may be simultaneously visible on the topic reading screen.** Everything else uses progressive disclosure.

This is a concrete, reviewable limit enforcing `09` Forbidden Pattern 01 (Feature Overload). The reading screen is the product; each control added to it costs attention. (Phase 4 patch to `09`.)

---

## Open items for implementation

1. ~~Verify all colour pairs against WCAG AA; adjust failures.~~ **Done 2026-07-11** — see Contrast audit above. Three tokens adjusted.
2. ~~Decide an interactive-boundary colour before the first form ships (Sprint 2).~~ **Done 2026-07-12** — `borderInteractive` added (light `#716E69`, dark `#8E8B85`; 4.54:1 and 4.50:1 at worst, against a 3:1 requirement). `border` and `borderStrong` remain **decorative only** — separators and card edges — and must never be the sole boundary of a control. `contrast.test.ts` enforces both halves of that.
3. ~~Verify the Turkish test string renders correctly in all three families at all weights.~~ **Done 2026-07-11** — rendered on the web target in Literata (body), Inter (headings) and JetBrains Mono (code). All Turkish diacritics correct, including the dotless `ı` and dotted `İ`. Native (Android/iOS) rendering is **not yet verified**.
4. Measure LCP impact of the preloaded body font on the static SEO surface (ADR-0009).
5. The `accent` hue (`#1D5D8C`) is the one value most open to personal preference — it is the product's signature colour and may be revised without affecting any other token.
