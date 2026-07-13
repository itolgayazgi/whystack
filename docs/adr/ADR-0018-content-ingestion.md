# ADR-0018 — Content Ingestion

- Status: Accepted
- Date: 2026-07-13
- Owner: WhyStack Core Team
- Sources: CLAUDE.md §1.3; `06-monorepo-structure.md` § Content Directory; `07-database-design.md`; `10-content-architecture.md`; ADR-0002; ADR-0004

---

## Context

`06` makes `content/**` the Single Source of Truth for educational content, and names three renderers
of it: the React Native app, the Knowledge Packs and the static site. `07` defines database tables for
topics, sections and relationships. `10` defines the Topic model.

**No document says how the Markdown becomes rows.** That gap is not academic: the answer decides where
content validation happens, what a bad topic breaks, and whether a typo fix is a content change or a
schema migration.

## Decision

1. **`content/**` remains the Single Source of Truth.** The database is a *projection* of it. Nothing is
   authored in the database, and no row survives that no file produced.

2. **A validate-then-import command moves content into SQL Server.** It runs in CI and in deployment.
   It validates first; **an invalid corpus never reaches the database at all** — not partially, not
   "mostly".

3. **Validation is deterministic, and it is a gate.** The rules live in `packages/knowledge-engine`;
   `tests/content-validation` applies them to the real corpus on every pull request. A violation fails
   the build.

4. **AI review is a report, never a gate.** `11`'s Terminology Agent and the review agents of `14` may
   score, critique and suggest. They may not approve. CLAUDE.md §1.5 stands: AI-generated content never
   publishes without human review.

5. **Terminology preservation is enforced mechanically.** Every dictionary term used in the canonical
   (English) text must survive verbatim into every translation; named mis-translations are rejected by
   name. This is a rule, not a judgement, and it is answered by a rule.

6. **The Markdown stays in files. The database stores metadata, relationships and publishing state.**
   `07` § Content Domain says this outright, and its tables carry `MarkdownPath` + `ContentHash` with no
   column for a body. The database is the *index* — identity, the Knowledge Graph, the editorial state,
   the things a query filters and sorts on. The words live in `content/`, where they can be reviewed in
   a pull request like everything else.

7. **The importer does not validate. It cannot see unvalidated content at all.** `pnpm content:validate`
   writes a **manifest** — and writes it only when the corpus passes. The C# importer reads that manifest
   and never opens `content/`. So there is one rule set, in one language, and an invalid corpus cannot
   reach the database because the file the importer needs does not exist.

## Amendments

**2026-07-14 — Decisions 6 and 7 replace two mistakes in the original text.**

The first version of this ADR rejected "the API reads `content/` from disk" on the grounds that it
*"contradicts `07`"*. **That justification was false.** `07` § Content Domain says the opposite —
*"Markdown may exist in files. The database stores metadata, relationships and publishing state"* — and
its tables are built for exactly that. The ADR was written without reading that section. `07` wins;
Decision 6 records it.

It also said the importer *"validates before it writes"*, which would have meant a second implementation
of every rule, in C#, alongside the TypeScript one. One fact in two languages: they agree for a year and
then disagree once, quietly, in the direction that lets bad content through. Decision 7 removes the
second implementation instead of promising to keep it in step.

## Alternatives Considered

- **EF migration seeds.** Rejected: a typo fix would become a migration, and content history would be
  tangled into schema history in the same files. It also makes content a deployment artefact rather
  than a reviewable one.

- **The Markdown body in the database.** Rejected: it contradicts `07`, and it puts the same text in two
  places. Whichever one is edited, the other is now wrong — and nothing would say which.

- **A second validator in C#, inside the importer.** Rejected: see the amendment above.

- **An AI agent as the terminology reviewer.** Rejected as a *gate*. "Does the Turkish text still say
  `Connection Pooling`?" is a fact with a correct answer, and a model answers facts *probably*. The
  cases where it is wrong are silent, fluent and indistinguishable from the cases where it is right —
  the worst possible property for a gate. The model keeps the job it is actually good at: judging
  whether the explanation is any good, and reporting.

## Consequences

- `packages/knowledge-engine` holds the Topic model and the rules, and **performs no I/O** — the React
  Native app imports it, and a `node:fs` inside it would be bundled into a phone build by Metro.
- `tests/content-validation` owns loading and is the CI gate.
- The importer (`apps/api/tools/`) validates before it writes, and writes nothing if validation fails.
- Section headings in Markdown are structural and stay English in every language; the heading a reader
  sees is a localized string chosen by section name. A translated heading would make `## Trade-Offs` and
  `## Ödünleşimler` two unrelated sections to every machine that reads them, and the mandatory-section
  check would pass a translation missing half the topic.

## References

- ADR-0002 (Canonical Topic Model), ADR-0004 (Knowledge Graph)
- `06`, `07`, `10`, `11`, `13`
