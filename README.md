# WhyStack

**An engineering learning platform that teaches *why* technologies exist — not just how to use them.**

- **Product name:** WhyStack
- **Repository name:** `whystack`
- **Status:** Sprint 0 complete · **Sprint 1 — Ready with Conditions**

WhyStack teaches engineering context: why a technology exists, which problem it solves, what trade-offs it introduces, and when *not* to use it. Content is version-aware, bilingual (Turkish/English), offline-capable, and always human-reviewed.

---

## 1. Repository Purpose

This repository is **documentation-driven**. The Foundation Pack is not background reading — it is the **constitutional layer** of the project. Implementation must comply with it; implementation never overrides it.

Every concept has **exactly one owning document**. If two documents disagree, that is a defect, not a choice.

---

## 2. MVP Scope

**In scope**

| Area | Decision |
|---|---|
| Backend | ASP.NET Core **modular monolith**, SQL Server, EF Core |
| Client | **One React Native application** → Web (RN-Web) + Android + iOS (ADR-0001) |
| Public web | **Static HTML** generated from `content/` for search-engine discoverability (ADR-0009) |
| Content | .NET backend ecosystem — one deep, interlinked cluster (DI → Service Lifetimes → Middleware → EF Core) |
| Auth | JWT access + rotating refresh token (ADR-0008) |
| Roles | `Guest`, `RegisteredUser`, `Administrator` active; others seeded but dormant (ADR-0005) |
| Learning | Reading, progress, bookmarks, quizzes, roadmaps |
| Developer Lab | 4–5 teaching tools (JWT Decoder, JSON Formatter, Regex Playground, Cron Builder) |
| Offline | Knowledge Packs (signed, checksum-verified) |
| AI | **Content production only** (ADR-0010) |
| Localization | Turkish / English — application language and content language are **independent** |

**Explicitly out of scope**

Runtime AI Learning Assistant (post-MVP, Premium) · Monetization of any kind (ADR-0012) · Architecture Explorer · Performance Lab · Senior Metrics · Community contribution · SSR · Graph database · Microservices · Redis · Kubernetes

---

## 3. Reading Order

**First time here — read in this order:**

| # | Document | Why |
|---|---|---|
| 1 | `01-product-vision.md` | What we are building |
| 2 | `02-product-principles.md` | The non-negotiables and the decision hierarchy |
| 3 | `04-development-roadmap.md` | What gets built, in what order |
| 4 | `05-system-architecture.md` | How the system is structured |
| 5 | `06-monorepo-structure.md` | Where every file belongs |
| 6 | `docs/adr/` | Decisions that **supersede** the documents above |

`00-project-discovery.md` and `03-philosophy.md` are context, not contracts. Read them once; do not consult them during implementation.

**Then read on demand** — only the document that owns the area you are touching (§4).

---

## 4. Document Ownership

**One document owns one area.** No document redefines another's area; it cross-references instead.

| Doc | Owns |
|---|---|
| `00-project-discovery.md` | Project discovery |
| `01-product-vision.md` | Product vision |
| `02-product-principles.md` | Product principles (constitution) |
| `03-philosophy.md` | Product philosophy |
| `04-development-roadmap.md` | Roadmap, sprint sequence, scope control |
| `05-system-architecture.md` | System architecture, layers, boundaries |
| `06-monorepo-structure.md` | Repository structure, file placement, naming |
| `07-database-design.md` | Database schema, migrations, indexing |
| `08-api-standards.md` | API contracts, errors, versioning |
| `09-ui-design-system.md` | UI rules, components, responsive, accessibility |
| `10-content-architecture.md` | **Topic model, Knowledge Graph, content rules** |
| `11-ai-content-pipeline.md` | AI workflow |
| `12-engineering-standards.md` | Coding standards; Coding / Implementation / PR Definition of Done |
| `13-quality-assurance.md` | QA, test strategy; Testing / Release / Production Definition of Done |
| `14-agent-ecosystem.md` | AI agent roles — *who* performs each responsibility |

### Single Sources of Truth

| Concept | Canonical owner |
|---|---|
| **Topic model & sections** | `10-content-architecture.md` (ADR-0002) |
| **Knowledge Graph** | `10-content-architecture.md` (ADR-0004) |
| **AI workflow** | `11-ai-content-pipeline.md` (ADR-0003) |
| **AI roles / who does what** | `14-agent-ecosystem.md` (ADR-0003) |
| **Definition of Done** | Split: `12` (build) + `13` (verify) (ADR-0006) |
| **Design token values** | `docs/design-system/design-tokens.md` (ADR-0013) |
| **Roles & permissions** | ADR-0005 → reflected in `05`, `07`, `08`, `13` |

> **Location:** the Foundation Pack lives in **`docs/sprint-0/`**, as specified by `06-monorepo-structure.md`.

---

## 5. Governance

When sources conflict, this order decides:

```
ADR  →  Foundation Pack  →  Engineering Standards  →  Implementation
```

**Implementation never overrides documented architecture.** If code and documentation disagree, the code is wrong — or an ADR is missing.

---

## 6. ADR Process

An **Architecture Decision Record** captures a decision that changes or supersedes the Foundation Pack.

**Write an ADR when a decision:** changes architecture · introduces a significant dependency or provider · changes data ownership · changes the deployment model · creates a long-term trade-off · reverses a previous decision.

**Format:** `docs/adr/ADR-00NN-short-title.md` — Status · Context · Decision · Alternatives Considered · Consequences · References. **Concise: 1–2 pages.** An ADR is a decision, not a design document.

### ADR Index

| ADR | Decision |
|---|---|
| [ADR-0001](docs/adr/ADR-0001-web-platform-strategy.md) | Web Platform Strategy — React Native Web, one app, no SSR |
| [ADR-0002](docs/adr/ADR-0002-canonical-topic-model.md) | Canonical Topic Model — `10` is the single source of truth |
| [ADR-0003](docs/adr/ADR-0003-ai-pipeline-architecture.md) | AI Pipeline Architecture — one AI system; `11` = workflow, `14` = who |
| [ADR-0004](docs/adr/ADR-0004-knowledge-graph.md) | Knowledge Graph — owned by `10`; SQL storage, no graph DB |
| [ADR-0005](docs/adr/ADR-0005-authorization-and-identity-model.md) | Authorization & Identity — 7 roles; 3 active in Sprint 1 |
| [ADR-0006](docs/adr/ADR-0006-definition-of-done.md) | Definition of Done — split between `12` and `13` |
| [ADR-0007](docs/adr/ADR-0007-monorepo-tooling.md) | Monorepo Tooling — pnpm workspaces + Turborepo |
| [ADR-0008](docs/adr/ADR-0008-authentication-strategy.md) | Authentication — JWT access + rotating refresh |
| [ADR-0009](docs/adr/ADR-0009-public-content-seo-surface.md) | Public Content SEO Surface — static generation from `content/` |
| [ADR-0010](docs/adr/ADR-0010-ai-scope-for-mvp.md) | AI Scope for MVP — content production only, no runtime assistant |
| [ADR-0011](docs/adr/ADR-0011-discoverability-and-ai-crawler-policy.md) | Discoverability & AI Crawler Policy — allow citation, deny training |
| [ADR-0012](docs/adr/ADR-0012-monetization-deferral.md) | Monetization Deferral — no revenue model in MVP |
| [ADR-0013](docs/adr/ADR-0013-typography-stack.md) | Typography Stack — Literata / Inter / JetBrains Mono |
| [ADR-0014](docs/adr/ADR-0014-licensing.md) | Licensing — MIT (code) / CC BY-SA 4.0 (content) / brand reserved |
| [ADR-0015](docs/adr/ADR-0015-product-name-and-brand-identity.md) | Product Name — **WhyStack**, `whystack.dev` |
| [ADR-0016](docs/adr/ADR-0016-react-native-toolchain.md) | React Native Toolchain — **Expo** + Expo Router (iOS from Windows) |

---

## 7. Sprint Lifecycle

Every sprint follows the same path (`04`):

```
Discovery → Specification → Architecture Review → Implementation
         → Testing → Documentation → Quality Review → Approval
```

A sprint is complete only when its **Exit Criteria** and both Definitions of Done pass (`12` build + `13` verify). Deferred scope must be written down — silent scope growth is the failure mode this process exists to prevent.

---

## 8. Implementation Workflow

1. **Read** the owning document for the area (§4) plus any relevant ADR.
2. **Confirm** the work is in scope for the current sprint (`04`). If not — stop.
3. **Plan** — files touched, tests required, risks. Stay inside the approved structure (`06`).
4. **Build** to `12-engineering-standards.md`. Complete every state: loading, empty, error, offline, disabled, permission.
5. **Verify** to `13-quality-assurance.md`. Negative cases, authorization, responsive, accessibility. Not just the happy path.
6. **Document** — update the owning document if behaviour changed. Write an ADR if a decision changed.
7. **Review** — both Definitions of Done must pass.

**If requirements conflict, stop and report the conflict. Do not choose silently.**

---

## 9. Claude Code

Claude Code operates from **[`CLAUDE.md`](CLAUDE.md)** — its operational manual: which documents to load and when, boundaries, forbidden actions, and the coding/review workflow.

`CLAUDE.md` **does not duplicate** this documentation. It references it. The Foundation Pack remains the single source of truth.

---

## 10. Repository Structure

Defined by **`06-monorepo-structure.md`**. Summary:

```
apps/          runnable applications (api, client)
packages/      shared code (ui, theme, localization, api-client, ...)
content/       canonical educational content — the product's core asset
docs/          documentation, ADRs, design system, QA
infrastructure/ docker, database, deployment, monitoring
scripts/       build, test, lint, content, database, release automation
tests/         integration, e2e, contract, content-validation, accessibility, performance
```

**No new top-level folder may be created without updating `06-monorepo-structure.md`.**

---

## 11. Contributing

See [`CONTRIBUTING.md`](CONTRIBUTING.md) (scope, standards, pull requests) and [`SECURITY.md`](SECURITY.md) (vulnerability disclosure — **never** report security issues as public issues).

Content contributions follow the editorial workflow in `10-content-architecture.md`. **AI-generated content never publishes without human review.** That rule has no exceptions.

---

## 12. Getting Started

**Prerequisites:** Node 24 (`.nvmrc`), pnpm 11, .NET SDK 10 (`global.json`), Docker.

```bash
pnpm install
./scripts/setup/dev-database.ps1     # SQL Server + connection string + migrations
pnpm api                             # API      → /health, /health/ready
pnpm --filter @whystack/client web   # Client   → http://localhost:8081
```

**Verify everything the way CI does:**

```bash
pnpm verify:all    # lint, design tokens, typecheck, 111 tests, .NET format + tests
```

No secret is ever written to a tracked file. `dev-database.ps1` generates the SQL Server password and
stores the connection string in .NET user secrets; `infrastructure/docker/.env` is gitignored.

---

## 13. Licensing

Full terms: **[`LICENSING.md`](LICENSING.md)**.

This repository contains **two different assets, and they are licensed differently** (ADR-0014). A single licence would either over-protect the code or under-protect the content.

| Asset | Licence | Why |
|---|---|---|
| **Code** — `apps/`, `packages/`, `scripts/`, `infrastructure/`, `tests/`, config | [**MIT**](LICENSE) | The code is not the moat. Principle 12 wants this repository to *teach*, and that requires it to be open and readable. |
| **Content** — everything under `content/` | [**CC BY-SA 4.0**](content/LICENSE) | The content **is** the product. Share-alike keeps it open: you may republish it, but you may not take it closed. |
| **Brand** — the name *WhyStack*, the logo, the brand identity | **All rights reserved** | This is the actual protection, and it is separate from copyright. |

**What that means in practice:** you may use this code, and you may republish the content with attribution under CC BY-SA. You may **not** call the result *WhyStack*, or imply it is affiliated with or endorsed by this project.

AI training on the content is **permitted** — deliberately, and consistently with ADR-0011.
