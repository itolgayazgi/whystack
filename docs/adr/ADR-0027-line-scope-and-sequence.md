# ADR-0027 — Hat, Kapsam and Sequence: the content taxonomy

- **Status:** **Proposed — awaiting the owner's approval. No code has been written against it.**
- **Date:** 2026-07-16
- **Deciders:** Tolga Yazgı (owner)
- **Derived from:** the owner's `whystack-backend-taksonomi.md` and `whystack-kapsam-katmani.md`
  (vendored at `docs/design-system/mockups/`)
- **Supersedes:** the `KnowledgeDomain` axis as currently seeded. **Amends ADR-0023 (SubArea) — see the
  open question, which must be answered before this is implemented.**
- **Related:** ADR-0021 (concept vs implementation), ADR-0024 (blocks), ADR-0025 (progress)

---

## Context

The roadmap draws lines. Until now "hat" meant **ecosystem** — the map's own legend said so
(*"Her hat bir ekosistem, her durak bir konu"*), and `/api/v1/roadmap` is built on it.

The owner's taxonomy says otherwise, and gives a reason the map cannot argue with:

> **Ekosistem (.NET) ← üst sekmelerden seçilir, ağın tamamını değiştirir**

An ecosystem is not a line through the network; it is **which network you are looking at**. Java is not a
parallel line beside .NET — it is the same eight lines, rebuilt. That is already how our content model
works (ADR-0021: the concept is written once; only the implementation slice changes), so the metaphor and
the model finally agree.

The lines are the eight **Bölüm** of Backend:

| # | Hat | Kapsar | Renk |
|---|---|---|---|
| B1 | Dil & Runtime | C#, CLR, bellek, async, tip sistemi | Altın (ana hat) |
| B2 | Web API & Framework | ASP.NET Core, HTTP, middleware, minimal API | Turuncu |
| B3 | Veri Erişimi | EF Core, Dapper, transaction, migration | Mavi |
| B4 | Mimari & Tasarım | Clean Arch, DDD, CQRS, pattern'lar | Mor |
| B5 | Mesajlaşma & Dağıtık | RabbitMQ, MassTransit, outbox, saga | Yeşil |
| B6 | Güvenlik & Kimlik | JWT, OAuth/OIDC, OWASP, secrets | Kırmızı |
| B7 | Test & Kalite | Unit/integration, mock, TDD, CI | Turkuaz |
| B8 | Performans & Gözlemlenebilirlik | Profiling, caching, logging, tracing | Bakır |

And a line is not flat: **EF Core is not a stop, it is an eight-stop neighbourhood** spread across zones
(Junior 2, Mid 4, Senior 2). Making it a menu level would take navigation to six tiers — *menü cehennemi*.
So depth arrives as **metadata, not navigation**:

```
Alan → Ekosistem → Hat → Durak → Blok        ← navigation, still four taps
                    │
                    └── Kapsam = durak grubu  ← metadata, drawn as a neighbourhood on the map
```

## Decision

### 1. `Hat` is a first-class axis, and it is the map's line

Ecosystem becomes what the design already draws it as: the tab that swaps the whole network. The roadmap
endpoint's `line` parameter means **hat**, and its colours come from the table above.

### 2. `Kapsam` (scope) groups stops into neighbourhoods

**A scope is 3–10 stops.** Fewer than three and it is not a neighbourhood — it is a stop. More than ten and
it splits in two. It is a label, never a menu level.

### 3. `Sequence` numbers a stop chain

A subject that will not fit in 20–25 minutes is not compressed; it is **split into numbered stops**
(Change Tracking I / II / III). The prerequisite chain already exists in the model.

```jsonc
{
  "line": "b3-data-access",
  "scope": "ef-core",
  "sequence": { "group": "change-tracking", "part": 1, "of": 3 }
}
```

### 4. The size rules are the content contract

| Length | Where it goes |
|---|---|
| < 5 min | `term` block (glossary card) |
| 5–25 min | one stop |
| 25–60 min | a stop chain (I, II, III…) |
| 3–10 stops | a scope |
| > 10 stops | the scope splits |

---

## The problem this exposes: our `KnowledgeDomain` axis conflates two things

This is not a detail. Our seeded domains are:

`backend` · `database` · `language` · `architecture` · `networking` · `devops` · `security` · `testing`

Under this taxonomy, **two of those are Alan and the rest are Hat**:

| Seeded as a "domain" | Actually |
|---|---|
| backend | **Alan** |
| database | **Alan** |
| language | B1 Dil & Runtime — a **hat inside Backend** |
| architecture | B4 Mimari — a **hat** |
| security | B6 Güvenlik — a **hat** |
| testing | B7 Test — a **hat** |
| networking | around B2/B5 — a **hat** |
| devops | unclear: Alan, or B8? |

So `Topic.DomainId` currently answers two different questions depending on the row, and the sidebar's
"Alanlar" rail lists lines and areas side by side as if they were peers.

**The cost of fixing it is as close to zero as it will ever be: there are two topics in the database, and
both are archived.** Every month of content makes this migration more expensive and the taxonomy less
true. Now is the moment.

Proposed: `KnowledgeDomain` is re-seeded as **Alan only** (Backend, Database, …), and `Line` becomes a new
table (`Key`, `Name`, `DomainId`, `Colour`, `SortOrder`) holding B1–B8. `Topic.LineId` replaces the
overloaded domain link.

---

## OPEN QUESTION — must be answered before any code

**Is `Kapsam` the same axis as ADR-0023's `SubArea`?**

The owner's reasoning when choosing a new axis was that they differ in granularity:

> *"Async/Await" ile "EF Core" aynı rafta duramaz çünkü farklı granülerlikte (biri konu inceliğinde,
> diğeri 3-10 duraklık mahalle).*

The evidence points the other way, and it is worth putting in front of him before we build:

| B3's scopes (his kapsam doc) | Our seeded SubAreas (ADR-0023) |
|---|---|
| SQL & Sorgu Temelleri (3 stops) | Async / Await |
| EF Core (8 stops) | Bellek Yönetimi |
| Micro-ORM & Ham SQL (3 stops) | Koleksiyonlar |
| **Transaction & Eşzamanlılık** (4 stops) | Hata Yönetimi |
| Şema & Yaşam Döngüsü (3 stops) | Dependency Injection |
| | **Eşzamanlılık** |

"Transaction & Eşzamanlılık" and "Eşzamanlılık" are the same shelf. And the two definitions are the same
sentence twice:

- ADR-0023 on SubArea: *the theme axis — "async" threads Junior → Expert instead of scattering across
  level buckets.*
- The kapsam doc on Kapsam: *EF Core zone'lara yayılır — Junior'da 2, Mid'de 4, Senior'da 2.*

If they are one axis, ADR-0023's `SubAreas` table is renamed and gains the 3–10 rule, and nothing else
changes. If they are two, a topic carries both a theme and a scope, and we owe an answer to the question
every editor will ask: *"which one does EF Core go in?"*

**Recommendation: one axis.** `SubArea` becomes `Scope`, keeps its Restrict FK and its studio management,
and gains the size rule. Two axes at the same granularity is a choice an editor has to make correctly on
every topic, forever, with nothing to tell them how.

---

## Consequences

- The roadmap, the metro map, its legend and `lineColors` are rewritten from ecosystem to hat.
  The ecosystem tabs stay exactly as they are — they already do what the taxonomy says they do.
- `Topic` gains `ScopeId` and a nullable `Sequence` (group, part, of).
- The map draws the scope bracket; "Hattım" groups stops under scope headings with a "4/8" counter; the
  stop's künye gains "Kapsam: EF Core · 5/8 durak".
- **Scope badges** (`EF Core ✓`) are the unit the live-content design hangs its notifications on — this
  ADR is a prerequisite for that work, not part of it.
- The seeded `SubArea` values are re-cut against B1–B8 rather than kept as they are; they were seeded
  before the taxonomy existed.
