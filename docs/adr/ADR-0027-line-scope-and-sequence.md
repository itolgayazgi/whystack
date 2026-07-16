# ADR-0027 — Hat, Kapsam and Sequence: the content taxonomy

- **Status:** **Accepted** — 2026-07-16, with both open questions closed by the owner (see *Resolutions*).
- **Date:** 2026-07-16
- **Deciders:** Tolga Yazgı (owner)
- **Derived from:** the owner's `whystack-backend-taksonomi.md` and `whystack-kapsam-katmani.md`
  (vendored at `docs/design-system/mockups/`)
- **Supersedes:** the `KnowledgeDomain` axis as currently seeded. **Supersedes ADR-0023: its `SubArea`
  becomes this ADR's `Scope`. One axis, one name.**
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

`KnowledgeDomain` becomes **`Area`** — areas only (Backend, Frontend, Database, DevOps) — and `Line` is a
new table (`Key`, `Name`, `AreaId`, `Color`, `SortOrder`) holding B1–B8. `Topic.LineId` replaces the
overloaded domain link; a topic's area is its line's area, asked once rather than stored twice.

Renamed rather than re-seeded in place: "Domain" is already the name of the innermost layer of this
codebase (`WhyStack.Domain`), and a `KnowledgeDomain` that no longer means what it says, sitting next to a
`Domain` namespace that means something else entirely, is two traps for the price of one.

---

## Resolutions — 2026-07-16

Both questions below were put to the owner before any code was written. His answers, and the reasoning he
gave, are recorded here because they are the decision:

**1. Kapsam and SubArea are one axis.** In his own words:

> *kusur benim dokümanımda: "kapsam" katmanını tasarlarken kod tabanında zaten SubArea ekseninin var
> olduğunu bilmiyordum ve aynı granülerlikte ikinci bir eksen icat etmiş oldum.*

The axis is named **`Scope` in code, "Kapsam" in the UI.** `SubArea` is retired outright — not aliased —
so two names for one thing cannot both stay alive. The kapsam document's rules bind here: the 3–10 rule,
the badge, the notification, the "7→8" counter, and ADR-0025's level-baseline snapshot.

**The existing seeds were not wrong — they were homeless.** They are scopes; what was missing was which
line they live on. They are re-parented rather than re-cut:

| Seed | Line |
|---|---|
| Async / Await, Bellek Yönetimi, Koleksiyonlar, Hata Yönetimi, Eşzamanlılık | B1 Dil & Runtime |
| Dependency Injection | B4 Mimari & Tasarım |

**The nuance that survives the merge, and why it matters.** Merging the *axis* does not merge the two
concurrency scopes. B1's **"Eşzamanlılık"** (the language's tools: threads, locks, async primitives) and
B3's **"Transaction & Eşzamanlılık"** (the data world: isolation levels, deadlocks) are different
neighbourhoods on different lines. They read like a duplicate and are not one.

This is written down so the question — *"neden iki yerde eşzamanlılık var?"* — has an answer the first time
somebody asks it, rather than a tidy-up that quietly destroys the distinction. A scope is only meaningful
inside its line: the same word in two contexts is two neighbourhoods, exactly as "Index" means one thing on
B3 and another in the Database area.

**2. `KnowledgeDomain` splits into Area + Line, this sprint.** The mapping is the owner's:

| Today | Becomes |
|---|---|
| backend, database | **Area** (with frontend, devops) |
| language | **B1** (Area = backend) |
| architecture | **B4** (Area = backend) |
| security | **B6** (Area = backend) |
| testing | **B7** (Area = backend) |

The sidebar follows: areas at the top, lines inside the area.

> `networking` has no home in this mapping and none in the taxonomy document. No topic uses it, so it is
> dropped with the old seed rather than guessed at. If it should be a line (B2's HTTP territory?) or an
> area, that is a decision still to make, and dropping it costs nothing today.

---

## The question this replaced (kept for the record)

**Was `Kapsam` the same axis as ADR-0023's `SubArea`?**

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

**Recommendation: one axis** — accepted. `SubArea` becomes `Scope`, keeps its Restrict FK and its studio
management, and gains the size rule. Two axes at the same granularity is a choice an editor has to make
correctly on every topic, forever, with nothing to tell them how.

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

---

## Follow-up — 2026-07-16: the ecosystem axis belongs to an area

The owner's `whystack-alan-taksonomisi.md` (vendored alongside this ADR's other sources) confirms this
model across all four areas and adds one constraint it does not yet satisfy:

| Area | What its ecosystem axis MEANS | Values |
|---|---|---|
| Backend | Language / platform | .NET · Java · Node.js · Python · Go · Rust |
| Frontend | Framework | React · Angular · Vue · Svelte |
| Database | Engine | SQL Server · PostgreSQL · MySQL · MongoDB |
| DevOps | Cloud | Cloud-agnostic · Azure · AWS · GCP |

**`Ecosystems` is flat.** It has no `AreaId`, so `.NET` is an ecosystem of everything — and the tab strip on
Frontend would offer .NET, Java and PHP. Nothing breaks today because Backend is the only area with lines,
which is exactly the kind of bug that waits for the second area and then looks like a UI mistake.

`Ecosystem.AreaId` (Restrict), and the tab strip reads the current area's ecosystems. Not done in this
ADR's migration: it is a separate change with its own seed, and folding it in would have made a migration
that already refuses unmappable rows into one nobody could review.

`ProgrammingLanguage` needs a second look at the same time. It exists under `Ecosystem` and models "C# in
.NET" — under this taxonomy the ecosystem IS the language for Backend, so the table may be a layer this
model no longer has. That is a question, not a decision.
