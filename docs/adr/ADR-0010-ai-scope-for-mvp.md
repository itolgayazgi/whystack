# ADR-0010 — AI Scope for MVP

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Sources: Confirmed decision (2026-07-11); `04-development-roadmap.md`, `05-system-architecture.md`, `08-api-standards.md`, `09-ui-design-system.md`, `11-ai-content-pipeline.md`

---

## Context

The Foundation Pack uses AI in **two structurally different ways**, with very different economics:

| | **Content Production AI** (`11` pipeline) | **Runtime Learning Assistant** (Sprint 9) |
|---|---|---|
| Trigger | Founder/editor authoring | Every user, every session |
| Cost profile | One-time, bounded, founder-controlled | Recurring, unbounded, user-driven |
| Order of magnitude | ~200 topics → tens of dollars, **once** | 1,000 DAU × 2 req/day → **hundreds to ~$1k per month** |
| Scales with traffic | No | **Yes — linearly** |

On a free product, every runtime AI request is pure loss, and cost grows with exactly the thing we want (traffic). Static content gets *cheaper* per user through caching; AI does not.

Critically, the runtime assistant's core action — *"explain this topic at Junior / Mid / Senior / Expert level"* — is keyed by `(topic, version, language, level)`. A thousand users requesting the same explanation are requesting **one** artifact, not a thousand. And the content model **already has Level** as a first-class concept (`07` `TopicLevels`, `10` "Level affects explanation depth").

Therefore the level explanations are not an AI feature at all — they are **content**.

## Decision

1. **MVP AI scope = content production only.** AI is used exclusively inside the `11-ai-content-pipeline.md` workflow, under founder control, with mandatory human review.
2. **Level variants (Junior / Mid-Level / Senior / Expert) are authored content**, produced at authoring time, human-reviewed, versioned and published like any other content. They are **not** runtime AI outputs.
3. **The runtime AI Learning Assistant is removed from the MVP.** Sprint 9 is deferred to post-MVP.
4. **Roadmaps remain curated, versioned, authored content** (`07` `Roadmaps/RoadmapStages/RoadmapNodes`). They are never generated per user. There is no per-user roadmap AI cost.
5. **Post-MVP:** the runtime assistant returns as a **`PremiumUser`** capability (the role is already reserved in ADR-0005), gated by hard quotas, per-user limits, and `(topic, version, language, level)` caching. It must be economically self-funding.

## What This Removes From MVP

Deferred with Sprint 9: AI rate limiting and quota system, provider fallback and failure states, streaming response UI, AI cost telemetry and budget alerts, AI panel UI, `POST /api/v1/ai/*` runtime endpoints.

## What This Preserves

- **Provider abstraction** — kept, so the assistant can return later without redesign (`05` Principle 07, `11` Rule 01).
- **AI labeling and human-review mandate** — unchanged; still applies to every AI-origin artifact in the editorial pipeline.
- **The Agent Ecosystem (`14`)** — unchanged. Its agents govern content and engineering work, which continue.
- **`07` AI tables** — `AiGeneratedDrafts` remains (content production). `AiProviders` / `AiUsageEvents` remain in the schema but are not exercised by runtime user traffic.

## Consequences

- **Runtime AI cost in MVP: $0.**
- **Level explanations work offline** (they ship inside Knowledge Packs) — impossible with a runtime assistant.
- **Level explanations are human-reviewed** — a strict improvement over unreviewed generated text, and better aligned with the Human Review philosophy.
- **Significant MVP simplification** — an entire sprint and its operational machinery leave the critical path.
- Monetization gains a natural, cost-justified first product: the assistant costs money, so it should *be* the paid feature.

## Alternatives Considered

- **Full runtime assistant as originally specified.** Rejected: recurring cost with no revenue, unbounded exposure to a traffic spike, and unreviewed content served to learners.
- **Limited runtime assistant (login + hard quota + cache).** Rejected *for MVP*: retains all of Sprint 9's operational complexity (quotas, fallback, streaming, telemetry) for marginal benefit over pre-generated, reviewed level variants. This is the shape the post-MVP Premium feature will take.

## References

- ADR-0005 (Authorization and Identity Model — `PremiumUser`)
- `04`, `05`, `08`, `09`, `11`, `13`, `14`
