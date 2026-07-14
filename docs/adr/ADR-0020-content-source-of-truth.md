# ADR-0020 — The Database Is The Source Of Truth For Content

- Status: Accepted
- **Supersedes: ADR-0018** (Content Ingestion)
- Date: 2026-07-14
- Owner: WhyStack Core Team
- Sources: `06-monorepo-structure.md`; `07-database-design.md`; `10-content-architecture.md`; `11-ai-content-pipeline.md`; ADR-0002, ADR-0004, ADR-0009, ADR-0018, ADR-0019

---

## Context

ADR-0018 made `content/**` the Single Source of Truth: topics are authored as Markdown files, reviewed in
a pull request, validated in CI, and imported into SQL Server. The database was a projection.

**That is a developer's workflow, and authoring content is not a developer's job.**

The product owner — who is the editor — needs to create a topic, say which ecosystem and which language it
belongs to, relate it to other topics, write its sections, and see whether it is valid *while he is
writing it*. Doing that through a text editor, a YAML file, a commit, a pull request and a twenty-minute
CI round trip is not a workflow anybody sustains past ten topics.

The two cannot both be the source of truth. One of them has to give.

## Decision

**1. SQL Server is the Single Source of Truth for content.** Topics are created and edited in the
application, by an authenticated editor, and saved through the API.

**2. `content/` becomes an EXPORT, not a source.** It is generated from the database and consumed by the
two things that need content as files:

```
        ┌──────────────┐
        │  SQL Server  │   ← the source. Authored in the app.
        └──────┬───────┘
               │  export
               ▼
        content/**              ← generated. Never hand-edited.
          ├─→ Astro static site      (ADR-0009 — public, indexable, zero JS)
          └─→ Offline Knowledge Pack (the archive a phone reads on a train)
```

The React Native app reads content from the **API**, not from files.

**3. The validation rules move to the API, and there is still only one implementation of them.**

ADR-0018's central property must survive: *invalid content cannot reach a reader.* But the gate is no
longer a CI job — it is the **save**. So the rules live where the gate is:

- `WhyStack.Application/Content/Validation` owns them. It is the ONLY implementation.
- `POST /api/v1/content/topics/{id}/validate` gives the editor live feedback **while writing** — the same
  rules, the same messages, before they hit save.
- The save itself re-runs them. Client-side validation is a courtesy; it is never the gate.

`packages/knowledge-engine` and `tests/content-validation` are removed. Their rules — mandatory sections,
terminology preservation, graph resolution, table cells that are facts and not paragraphs (ADR-0019) —
move across intact, with their tests.

**4. Nothing about the review gate changes.** `10`'s lifecycle stands. An `AiDraft` still cannot become
`Published` without a human moving it through review, and CLAUDE.md §1.5 still holds: AI-generated content
never publishes without human review. The gate moved; it did not open.

**5. The export is committed, and that is how content keeps a history.**

`content/` is written on publish and committed. It is **never hand-edited and never authoritative** — it is
a snapshot of what is published, and git holds it for the same reason git holds anything: so that "what
changed, when, and who did it" has an answer six months later.

A fresh environment — CI, a new machine, a restored backup — is filled from that snapshot. So the importer
survives, in a strictly narrower job:

```
  AUTHORING       app → API → SQL Server        (the only way content is written)
  RECORD          SQL Server → content/ → git   (one-directional, on publish)
  BOOTSTRAP       content/ → SQL Server         (an empty database, never a merge)
```

This is not two sources of truth. A source of truth is what you *edit*; nobody edits the export, and the
bootstrap only ever runs against a database with nothing in it. The direction is the whole guarantee.

## What this costs, stated plainly

**Content loses pull-request review.** A bad edit to a published topic is no longer caught by a reviewer
reading a diff before it lands — it lands, and the export records that it landed.

So the database takes over the job, and this ADR is not complete without it:

- **Every content change is recorded** — who, when, from what, to what (`07` § Editorial Workflow Domain,
  which already models this and was never built).
- **A published topic cannot be silently overwritten.** `07` says so already; the editorial state machine
  is what makes it true.
- The lifecycle gate is unchanged: a draft still needs a human to move it to `Published`.

If content review later needs to be a diff a human approves *before* it lands, the export is the material
for it — but it will be a deliberate feature, not a side effect of where the files happened to live.

## Alternatives Considered

- **Keep the files; have the API commit to git.** Rejected. The API would need a git identity, every save
  would be a commit, and "save a draft" — which an author does twenty times an hour — would produce twenty
  commits. It preserves the *form* of pull-request review while destroying what makes it useful.

- **Author in the app, sync back to files as the source.** Rejected: that is two sources of truth wearing
  a trench coat. The first time they disagree — and they will, on the first concurrent edit — nothing can
  say which one is right.

- **Re-implement the validator in C# and keep the TypeScript one for CI.** Rejected, and this is the
  mistake this repository has already made once (two homes for one fact, agreeing for a year and then
  disagreeing quietly). One implementation, in the layer that owns the gate.

## Consequences

- ADR-0018 is superseded. Its Decisions 1, 2 and 3 are reversed; its Decisions 4 (AI review is a report,
  never a gate), 5 (terminology enforced mechanically) and 6 (Markdown is not a `<script>`) survive and are
  restated here.
- `10-content-architecture.md` § Content Directory is patched: `content/` is a build output.
- `06-monorepo-structure.md` keeps `content/` as an approved directory — it still exists, it is just
  generated.
- `apps/api/tools/WhyStack.ContentImport` is replaced by an **exporter**: database → `content/`.
- The offline Knowledge Pack and the static site are unaffected in shape. They still read files; the files
  are simply produced rather than written.

## References

- ADR-0018 (superseded), ADR-0019 (Teaching Structure), ADR-0009 (Static site), ADR-0002, ADR-0004
- `07` § Content Domain, § Editorial Workflow Domain · `10` · `11` · CLAUDE.md §1.5
