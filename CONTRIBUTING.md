# Contributing to WhyStack

Thank you for looking. Before you write anything, read this — the project is
**documentation-driven**, and that changes what "a good contribution" means here.

## The one rule that surprises people

**The Foundation Pack in `docs/sprint-0/` is the source of truth, not the code.**

If the code and a document disagree, the *code* is wrong — or an ADR is missing.
A pull request that improves the code but contradicts a document will be asked to
either follow the document or change it first.

Governance order:

```
ADR  →  Foundation Pack  →  Engineering Standards  →  Implementation
```

**If two documents disagree, that is a defect. Report it — do not pick one.**

## Before you open a pull request

1. **Check the scope.** `docs/sprint-0/04-development-roadmap.md` says what is in
   the current sprint. Work outside it will be deferred, however good it is.
2. **Read the owning document** for the area you are touching. Every concept has
   exactly one owner — the table is in the README.
3. **Find the file's approved location** in `06-monorepo-structure.md`. If your
   file has no approved home, open an issue rather than inventing a folder.

## Standards

- **Code:** `12-engineering-standards.md`. This owns the Coding, Implementation
  and Pull Request Definition of Done.
- **Tests:** `13-quality-assurance.md`. This owns the Testing and Release
  Definition of Done. **Both must pass.**
- Happy-path-only tests are not sufficient. Negative cases, authorization and
  validation are part of the work, not a follow-up.
- **No hardcoded design values.** Colour, spacing, type and motion come from
  `packages/theme`.
- **No hardcoded educational content** in application code. Content lives in
  `content/`.
- **Never hide a failure.** No swallowed exceptions, no silent fallbacks. A
  language or version fallback must be visible in the response *and* in the UI.

## Content contributions

Content follows the editorial workflow in `10-content-architecture.md`.

**AI-generated content never publishes without human review.** That rule has no
exceptions, and a pull request that moves content from `AiDraft` straight to
`Published` will be rejected.

Approved technical terminology (`Middleware`, `Dependency Injection`,
`Garbage Collector`, …) is **never translated**. Explanations are localised; the
term is not.

## Licensing of your contribution

By contributing you agree that:

- **Code** contributions are licensed under **MIT** (see `LICENSE`).
- **Content** contributions are licensed under **CC BY-SA 4.0** (see
  `content/LICENSE`).

There is no CLA today. One may be introduced before community contribution opens
(ADR-0014).

## Reporting security issues

**Not as a public issue.** See `SECURITY.md`.
