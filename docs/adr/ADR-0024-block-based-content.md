# ADR-0024 — A Topic Is A Sequence Of Blocks, Not Fixed Sections

- Status: Accepted
- Date: 2026-07-16
- Owner: WhyStack Core Team
- Sources: `whystack-konu-iskeleti.md` (the author's content-skeleton design); the four UI mockups (basamak home, metro map, durak-içi reading screen); `09-ui-design-system.md`; `10-content-architecture.md`; ADR-0002 (Canonical Topic Model); ADR-0019 (Teaching Structure); ADR-0021 (Concept and Implementation); ADR-0022 (Separate Web Application); ADR-0023 (Sub-Area)
- Revises: ADR-0002 (section structure → blocks), ADR-0021 (per-ecosystem implementation → ecosystem-tagged blocks). Refines ADR-0019 (the problem-first weave becomes the `hook` block).

---

## Context

The section model gave a topic a fixed set of labelled slots — thirty of them, twelve mandatory. Authoring the
first real topic proved the model wrong: filling twelve labelled boxes produces a form, not writing, and the
meaning fragments across the headings (the complaint that opened this whole thread). A five-tab collapse was
drafted (a since-deleted ADR-0024) and abandoned, because it was still the same section thinking wearing fewer
labels.

The author then designed a different model — **archetype + block** — and it is the right one. No good technical
explanation is a fixed template: "async/await" (a mechanism) and "REST vs gRPC" (a comparison) have genuinely
different shapes. What they share is a small vocabulary of *building blocks* — a hook, a code sample, a
misconception, a checkpoint — composed differently per topic. That is how the field actually explains things,
and it maps cleanly onto our two-platform architecture (ADR-0022): one block is one unit of data that both web
and mobile render.

## Decision

**A topic's body is an ordered sequence of typed BLOCKS. The archetype decides which blocks the skeleton
starts with; the author composes the rest.**

### 1. Blocks replace sections

`TopicSection`, `TopicImplementation`, `ImplementationSection` and the thirty `SectionType` rows are retired. A
topic version owns an ordered list of `TopicBlock`, each of which is:

- **BlockType** — one of twelve (below).
- **Order** — position in the flow.
- **LanguageCode** — the EN and TR sequences run in parallel, block for block.
- **EcosystemKey** — nullable (see §4).
- **DataJson** — the block's content, shaped by its type.

### 2. Twelve block types

`hook`, `story`, `concept`, `code`, `diagram`, `compare`, `myth`, `checkpoint`, `prod`, `term`, `summary`,
`next`. Each does one job; each has a small typed `DataJson` shape (e.g. `checkpoint` = `{question, options[],
correct, explanation}`; `code` = `{file, lang, source, highlightLines[], annotation}`).

### 3. Archetypes drive the skeleton, four blocks are always required

Six archetypes — `Concept`, `Mechanism`, `Comparison`, `Incident`, `Pattern`, `Workshop` — each define a
suggested block order. Regardless of archetype, four blocks are mandatory before publish (the completeness gate,
`ValidateForReview`):

- `hook` — a topic opens with a **question**, never a definition ("why before how", ADR-0019). The `hook` IS the
  problem-first weave that the section model kept fragmenting.
- at least one `checkpoint` — passive reading is not learning.
- `summary` — what the reader keeps.
- `next` — no dead ends; every station has a continuation.

`Archetype` is added to `Topic`. **`Category` stays** (ADR-0021's classification): archetype is the *shape of the
explanation*, category is the *subject* (Performance, Security) — orthogonal, and category still drives
discovery and search.

### 4. A topic is one concept; blocks carry an optional ecosystem tag

The topic identity stays ecosystem-agnostic (ADR-0021's promise: *the reason transfers*). But a block may be
tagged to an ecosystem:

- **Untagged blocks are shared** — the `hook`, the "why", the mental model. Written once.
- **Tagged blocks are per-ecosystem** — the mechanism, the code, the ecosystem-specific mistakes. `async/await`
  is a state machine in .NET and virtual threads in Java; those are different blocks, tagged `dotnet` and
  `java`.

A reader on the .NET line sees *untagged blocks + `dotnet` blocks*, merged in order. This is ADR-0021 evolved:
the implementation is no longer a code snippet in a fixed slot but a full run of blocks. It costs one nullable
column, and it keeps the shared "why" — the product's thesis — in exactly one place, where it cannot drift
between ecosystems.

### 5. Progress is per (topic, ecosystem) — but not built yet

"Finishing the .NET line" means completing each station's .NET treatment, so progress is keyed by (topic,
ecosystem, block). The schema is designed with this in mind, but **progress, the metro map and streaks are NOT
in this ADR** — they are Sprint 4/5. And per the author's decision, **streaks and forced topic locking are
deliberately excluded**: `09` warns against gamification ("Not a gamified social application"), and the product
keeps progress and checkpoints (pedagogical) without streaks or locks (engagement mechanics). That balance can
be revisited in its own ADR; it is not assumed here.

### 6. DataJson is validated in the application layer

The block body is a schemaless `nvarchar(max)` JSON column — flexibility the section model's fixed columns could
not give. The cost is that the database no longer validates shape, so each block type's schema is enforced in
`WhyStack.Application` (the same layer that owns the completeness rules), on every save. A schemaless column with
no application gate would be a way to store a `checkpoint` with no correct answer; the gate is not optional.

### 7. One JSON, two platforms

A block is rendered from the same `DataJson` on web (single-page flow + a block-map scrollspy) and mobile (each
block a segment, the top bar filling block by block) — ADR-0022's shared client. The mockups are the
specification for both surfaces; the mobile topic is not a shortened web topic, it is the same blocks in smaller
bites.

## Consequences

- The author composes a topic from blocks an archetype suggests, instead of filling thirty boxes. The
  fragmentation that started this is gone.
- Interactive learning (checkpoints with structured answers) becomes first-class, not a Markdown afterthought.
- Existing content is migrated: the two authored topics' sections fold into blocks (`WhyItExists` → a `concept`
  block, etc.), so nothing published is lost.
- The reader screen — which never existed on web — is built once, block-driven, to the mockups.
- Sub-areas (ADR-0023), relationships (ADR-0004), terminology (ADR-0023's dictionary) and versioning (ADR-0020)
  all stand unchanged; blocks hang off the existing `TopicVersion`.

## Alternatives rejected

- **Keep sections, collapse to five tabs.** The drafted-and-deleted approach. Still fixed slots; still
  fragments; does not enable interactive blocks or per-block ecosystem tagging.
- **A topic per ecosystem (fully separate).** Simpler per topic, but duplicates the shared "why" across
  ecosystems — the exact drift ADR-0021 exists to prevent, in the one place the product cannot afford it. The
  overlap is small, but keeping it shared costs one nullable column, so there is no trade to make.
- **Archetype replaces Category.** Rejected: they answer different questions (shape vs subject), and dropping
  category loses subject-based discovery and needs another migration.
- **Streaks and locks now.** Rejected for this ADR; a gamification decision `09` cautions against, deferred to
  its own ADR so it is made consciously rather than inherited from a mockup.
