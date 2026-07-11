# ADR-0003 — AI Pipeline Architecture

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Sources: Decision 04; `11-ai-content-pipeline.md`, `14-agent-ecosystem.md`

---

## Context

Two overlapping "AI agent" systems existed: the content-generation agents inside `11-ai-content-pipeline.md` and the governed engineering agents inside `14-agent-ecosystem.md`. They defined overlapping roles (Architecture/Security/Performance reviewers, Code Example, Technical Reviewer) twice, so it was unclear which governed a given task. There is one AI system, not two.

## Decision

1. There is **one AI system**.
   - **`11-ai-content-pipeline.md` owns the WORKFLOW** (the stages a request passes through).
   - **`14-agent-ecosystem.md` owns WHO performs each stage** (roles, responsibilities, permissions, human review). The Agent Ecosystem is **not a second AI pipeline**.
2. The **canonical top-level pipeline** is:

   `AI Provider → Context Builder → Prompt Builder → RAG → Structured Generation → Validation → Human Review → Publication`

3. **Step interpretation** (to keep the order coherent and lossless):
   - **Prompt Builder** constructs the prompt *skeleton* (system / role / product / editorial / task / output-schema layers).
   - **RAG** performs retrieval and injects grounded context into that skeleton before generation. "Grounding" language in `11` maps to this step.
   - **Validation** absorbs the previous *Quality Scoring* sub-step (automated, deterministic-first).
   - **Human Review** absorbs the previous *Editorial Approval* sub-step. No stage is lost; the top-level list is exactly the eight steps above.
4. Provider independence is preserved: no domain/application module depends on a provider SDK. The canonical approved-provider list lives in `11` (see ADR note below).
5. `11`'s embedded agent/role lists are reduced to **references** to `14`. Role definitions live in `14` only.

## Alternatives Considered

- **Keep two agent taxonomies, cross-referenced.** Rejected: duplicate "who," ongoing drift.
- **Merge 11 and 14 into one document.** Rejected: violates Decision 10 (one primary area per document); workflow and org are distinct concerns.

## Consequences

- `11` lifecycle wording is aligned to the eight-step order; its "AI Roles"/agent lists become references to `14` (Phase 4 patch).
- The approved-provider list is de-duplicated to one canonical list in `11` (resolves the earlier Gemini/OpenAI/Azure-OpenAI vs Gemini/Claude/GPT/DeepSeek inconsistency). Recommended canonical set for MVP: **Gemini (initial), plus provider-abstraction seams for OpenAI, Claude, Azure OpenAI**; any further providers require a doc update.
- Human review remains mandatory before publication (Human Review philosophy preserved).

## References

- Decision 04, Decision 10
- `11`, `14`
