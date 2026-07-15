# ADR-0023 — A Topic Carries A Sub-Area (Theme), Orthogonal To Level

- Status: Accepted
- Date: 2026-07-15
- Owner: WhyStack Core Team
- Sources: `04-development-roadmap.md` § Sprint 5 (Roadmap Engine); `10-content-architecture.md` § Content Categories; ADR-0021 (Concept and Implementation)
- Amends: ADR-0021 (adds one axis to the concept's metadata)

---

## Context

A topic already carries three independent axes:

- **Domain** — Backend, Database, Networking. The concept's home (ADR-0021).
- **Category** — Concept, Performance, Security, Syntax. The *kind* of topic it is.
- **Level** — Junior, Mid, Senior, Expert. Who it is written for.

None of them captures a **theme that deepens across levels.**

`async/await` is the worked example. It appears at every level, and it is a different topic each time:

- **Junior** — *usage*: `await` a call, do not block.
- **Mid** — *the traps*: deadlocks, `ConfigureAwait`, sync-over-async.
- **Senior** — *the machinery*: `ValueTask`, `Channels`, allocation under load.

Split C# only into level buckets and the `async` thread scatters into four separate buckets, mixed with
`LINQ`, `collections`, `generics` and everything else at that level. The reader loses the one sensation the
product is trying to create: **"this is the same idea, getting deeper."** The Category axis does not rescue
it either — a Mid `async` deadlock topic is `Category = Concept` or `Performance`, the same as fifty unrelated
topics. Category says *what kind of topic*; it does not say *which recurring thread*.

The roadmap makes the gap concrete. A roadmap is **level-major**: a Mid reader walks Mid nodes in order. But
the moment someone asks *"show me `async` from Junior to Expert,"* the corpus has no field to group by. The
grouping cannot be reconstructed from Domain + Category + Level — those describe the node, not the thread it
belongs to.

## Decision

**Add one axis to the concept: `SubArea` — a theme a topic optionally belongs to.**

1. **A new content-model axis, not a repurposed one.** `SubArea` is orthogonal to Domain, Category and
   Level. A topic has at most one. It is **nullable**: a standalone topic — "What is a transaction?" — belongs
   to no thread, and forcing one would be a lie, exactly as ADR-0021 made implementations optional rather than
   universal.

2. **A controlled vocabulary, because the whole point is grouping.** The slice — "`async` across levels" —
   requires a stable key to group by. Free text fragments (`async` vs `asynchrony` vs `asenkron`) and the
   slice silently splits. So `SubArea` is a table with a stable `Key`, and `Topic.SubAreaId` is a foreign key
   to it — the same shape as `KnowledgeDomain`, enforced by the database rather than by hope.

3. **The vocabulary is curated in the studio, not seeded in code.** Themes number in the dozens per domain and
   grow with the corpus — they are *data*, like content, not *code*, like the editorial ladder. An editor
   manages them on a studio screen (the same shape as the terminology dictionary), then tags a topic from a
   dropdown. Seeding a fixed enum would force a migration per theme and block an author mid-write; making it a
   free string would fragment the vocabulary the slice depends on. The studio-managed table is the only option
   that is both frictionless and clean.

4. **A theme in use cannot be deleted.** `Topic.SubAreaId → SubArea` is `ON DELETE RESTRICT`. Deleting a theme
   that still tags topics would silently untag them — a data-loss dressed as a tidy-up. The editor is told how
   many topics use it and must retag them first. (`SET NULL` was rejected for exactly this silence.)

5. **The field ships now (Sprint 3); the slice consumes it later (Sprint 5).** `SubArea` is content-model
   metadata — Sprint 3 territory — so it is added, authored and displayed now, and content is tagged from the
   first topic. The *"slice by theme"* query belongs to the Roadmap Engine (Sprint 5); when it arrives, the
   data is already there. This ADR does not build the slice. It builds the field the slice will need, so that
   Sprint 5 is a read, not a migration and a backfill.

## Consequences

- One nullable column on `Topics`, one small reference table, one studio screen, one dropdown. The cost the
  request estimated — "a single field" — holds.
- The roadmap stays level-major by default and gains a theme cross-section for free when the engine lands.
- A topic with no theme is normal and carries no warning — the null is a fact, not an omission.
- Deleting a theme is a deliberate act with a precondition, not a silent cascade.

## Alternatives rejected

- **Overload `Category`.** Category is the topic's *kind*; a theme is a *thread*. Merging them would make
  "the async thread" and "the performance kind" the same field, and neither query could be expressed.
- **A free-text tag on the topic.** Zero friction, but `async`/`asenkron`/`asynchrony` fragment, and the slice
  the field exists for splits without anyone noticing — the worst kind of wrong.
- **A seeded enum.** Themes are growing data; an enum makes every new theme a code change and a migration, and
  blocks an author until it ships.
- **Multiple themes per topic (many-to-many).** More than the request asked for, and it dilutes the slice: a
  topic tagged with three themes appears in three cross-sections, and "the async path" stops being a path.
  One theme per topic, revisited only if a real need appears.
