# ADR-0002 — Canonical Topic Model

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Sources: Decision 01, Decision 02, Decision 06; `10-content-architecture.md`, `07-database-design.md`

---

## Context

The Topic model was defined in six places with incompatible section lists (`00`, `01`, `02`, `07`, `10`, `14`), and the database `SectionType` enum in `07-database-design.md` could not represent several sections mandated by `10-content-architecture.md` (e.g. Core Mental Model, Trade-Offs, Alternatives). This is the product's most-reused schema and must have exactly one source of truth.

## Decision

1. **`10-content-architecture.md` is the Single Source of Truth for the Topic model** (structure, mandatory sections, section semantics). Every other document references it and must not redefine it.
2. The database **`SectionType` is a reference/lookup table, not a closed enum**, so new educational sections can be added without a breaking schema change (per Decision 02: "must never limit future educational sections").
3. `SectionType` is **seeded with at least** the Decision 02 set:

   `Introduction, Problem, MentalModel, CoreConcept, Architecture, Workflow, Example, CodeExample, TradeOffs, Alternatives, BestPractices, Performance, Security, CommonMistakes, InterviewQuestions, Quiz, Summary, References, RelatedTopics, Prerequisites, NextTopics, Glossary`

4. `10-content-architecture.md`'s authoring blueprint remains the human-facing guide. Any blueprint section not present in the seed set is **added to the reference table**, never dropped. The reference table is the canonical machine-readable projection of the doc-10 blueprint.
5. **Relationship-derived sections are not free text.** `Prerequisites`, `RelatedTopics` and `NextTopics` are **rendered projections of Knowledge Graph edges** (ADR-0004), and `Glossary` is a projection of terminology entries. The DB stores the relationship/terminology once; these sections present it. They must not be stored as duplicate free-text bodies.

## Alternatives Considered

- **Closed `SectionType` enum matching only doc 10's current list.** Rejected: violates Decision 02's extensibility requirement and forces migrations for every new section type.
- **Let each document keep its own list, reconciled by convention.** Rejected: this is the exact defect being removed.

## Consequences

- `07-database-design.md` `SectionType` becomes a seeded reference table (Phase 4 patch).
- `00`, `01`, `02`, `11`, `14` reduce their Topic-structure lists to a reference to `10` (Phase 4 patches).
- Content validation (`13-quality-assurance.md`, `tests/content-validation/`) validates against the doc-10 blueprint + reference table — one rule set, not six.
- Prevents the DB from ever being unable to store a mandated section again.

## References

- Decisions 01, 02, 06
- ADR-0004 (Knowledge Graph)
- `07`, `10`
