# ADR-0006 — Definition of Done

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Sources: Decision 05; `12-engineering-standards.md`, `13-quality-assurance.md`

---

## Context

A "Definition of Done" was defined in both `12-engineering-standards.md` and `13-quality-assurance.md`, with overlapping but non-identical checklists. Two DoDs cause drift and ambiguity for reviewers and Claude Code.

## Decision

1. The Definition of Done is **split by ownership**; no duplication is permitted.
2. **`12-engineering-standards.md` owns:**
   - Coding Definition of Done
   - Implementation Definition of Done
   - Pull Request Definition of Done
3. **`13-quality-assurance.md` owns:**
   - Testing Definition of Done
   - Release Definition of Done
   - Production Readiness Definition of Done
   - Definition of **Ready** (remains in `13`).
4. **Boundary rule (resolves the UI overlap):**
   - *Building* UI states (loading/empty/error/offline/disabled/permission) and responsive/accessible **implementation** → **`12`** (Implementation DoD).
   - *Validating/testing* those states across the device matrix, themes, dynamic text, screen readers → **`13`** (Testing DoD).
5. A task is "Done" only when **both** the applicable `12` DoD and `13` DoD are satisfied. Neither document restates the other's items; each links to the other.

## Alternatives Considered

- **Single unified DoD in one document.** Rejected: both engineering and QA need a DoD lens; Decision 10 assigns distinct ownership, so a single-doc DoD would force one document to own the other's area.

## Consequences

- `12` removes its testing-completion block and links to `13` (Phase 4 patch).
- `13` removes coding/implementation-completion overlap and links to `12` (Phase 4 patch).
- CLAUDE.md references both DoDs as a single combined gate.

## References

- Decision 05, Decision 10
- `12`, `13`
