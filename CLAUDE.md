# CLAUDE.md — Operational Manual

This is **not documentation.** It is the operating manual for Claude Code in this repository.

The Foundation Pack is the single source of truth. This file tells you **which document to open, when, and what you may not do.** It never restates their content — if a rule lives in a Foundation document, this file points to it.

**Governance:** `ADR → Foundation Pack → Engineering Standards → Implementation`. Implementation never overrides documented architecture.

---

## 1. Non-Negotiables

These apply to every task. No exceptions, no "just this once."

1. **Never invent project rules.** If the answer is not in the docs, **stop and ask.** Do not choose silently.
2. **Never create a top-level folder** not defined in `06-monorepo-structure.md`.
3. **Never hardcode educational content** in application code. Content lives in `content/`.
4. **Never commit secrets.** Placeholders must be obviously fake.
5. **AI-generated content never publishes without human review.** No exceptions.
6. **Never hide a failure.** No swallowed exceptions, no silent fallbacks, no claiming a test ran when it did not.
7. **Never hide a fallback from the user.** Language and version fallback must be visible in the response and the UI.
8. **Never hardcode design values.** Use tokens from `packages/theme` / `docs/design-system/design-tokens.md`.
9. **If two documents conflict — stop and report it.** A conflict is a defect, not a decision for you to make.

---

## 2. Document Loading Strategy

The Foundation documents are large (3,000–4,600 lines each). **Do not load them speculatively.** Load only the owner of the area you are touching.

**Always in context:** this file.

**Load by task:**

| Task | Load |
|---|---|
| Any code change | `12-engineering-standards.md` + `06-monorepo-structure.md` |
| Backend / API endpoint | `08-api-standards.md` (+ `05` for layer boundaries) |
| Database / migration | `07-database-design.md` |
| UI screen or component | `09-ui-design-system.md` + `docs/design-system/design-tokens.md` |
| Content authoring / schema | `10-content-architecture.md` |
| AI / content pipeline | `11-ai-content-pipeline.md` + `14-agent-ecosystem.md` |
| Tests / QA / release | `13-quality-assurance.md` |
| Scope question ("is this in this sprint?") | `04-development-roadmap.md` |
| Any architectural question | `docs/adr/` **first** — ADRs supersede the Foundation Pack |

**Never load during implementation:** `00-project-discovery.md`, `03-philosophy.md`. They are context, not contracts, and will crowd out actionable rules.

**Always check `docs/adr/` before trusting a Foundation document** on architecture, roles, AI scope, web platform or Definition of Done — several ADRs supersede text still present in those files.

---

## 3. Boundaries

**Layers** (`05`, `12`) — violating these is a blocking defect:

- Endpoints: bind → validate → authorize → call use case → map response. **No business logic. No `DbContext`.**
- Application: use cases. **No ASP.NET Core, no EF Core details, no provider SDKs.**
- Domain: rules and invariants. **Depends on nothing.**
- Infrastructure: EF Core, SQL Server, AI providers, storage. **Implements Application contracts.**

**Client** — UI components render; they do not own domain decisions, call the database, or contain provider logic.

**Content** — applications *render* content. They never *own* it.

**Files** — every file has one approved location (`06`). If you cannot place a file inside the approved structure, **stop and request a structural decision. Do not improvise.**

---

## 4. Forbidden Actions

Hard stops. If a task appears to require one of these, stop and report.

- Creating an undocumented top-level folder or a duplicate module (`Topics/` + `TopicManagement/`)
- `DbContext` in an endpoint · business rules in UI · provider SDK in Application
- `.Result` / `.Wait()` in an async path
- Unbounded queries (no pagination / no limit)
- Custom error shapes — Problem Details only (`08`)
- Numeric enum values on the wire — strings only (`08`)
- Local time in storage — UTC only, `...Utc` suffix
- Storing raw tokens or logging tokens, secrets, JWTs, passwords, connection strings
- Publishing content that skipped review · transitioning `AiDraft → Published`
- Introducing microservices, Redis, Kubernetes, a message queue, or a graph database
- Introducing SSR into the client app (ADR-0001) or a second web application
- Adding a runtime AI feature (ADR-0010 — MVP AI is content production only)
- Adding advertising, billing, or paywalls (ADR-0012)
- Translating approved technical terminology (`Middleware`, `Dependency Injection`, `Garbage Collector`, …)
- Adding an automatic popup, unrequested AI panel, gamification, or attention animation
- Claiming validation, coverage or a passing test that did not actually run

---

## 5. Coding Workflow

1. **Scope check** — is this in the current sprint (`04`)? If not, say so before writing code.
2. **Read the owner** — the document that owns this area (§2), plus relevant ADRs.
3. **Plan** — files, tests, risks, boundaries touched. Non-trivial work gets a plan before code.
4. **Build** to `12`. For user-facing work, implement **every** state: loading, empty, error, offline, disabled, permission.
5. **Test** to `13`. Negative cases, authorization, validation — **happy-path-only is insufficient.**
6. **Document** — update the owning document if behaviour changed. Write an ADR if a *decision* changed.
7. **Report** — what you built, what you tested, what you could **not** validate, known limitations.

**Definition of Done is split** (ADR-0006): `12` covers Coding / Implementation / Pull Request. `13` covers Testing / Release / Production Readiness. **Both must pass.**

---

## 6. Review Workflow

Review in this priority order (`12`):

```
Correctness → Security → Data Integrity → Architecture
→ Behaviour → Performance → Maintainability → Style
```

Style comments must never bury a correctness or security issue.

Before requesting review, self-review for: debug code, accidental secrets, missing tests, unhandled states, boundary violations, hardcoded design values.

**Agent boundaries** (`14`): agents propose; **humans approve.** You may analyse, generate, and review. You may **not** approve architecture, publish content, execute a destructive migration, deploy, rotate secrets, or escalate a role. Those are human decisions, always.

---

## 7. When To Stop And Ask

Stop — do not guess — when:

- Two documents disagree, or a document disagrees with an ADR.
- The task requires a decision no document has made.
- A file has no approved location under `06`.
- The work needs a new dependency, folder, layer, or pattern.
- The task appears to be out of the current sprint's scope.
- A requirement is ambiguous in a way that changes the implementation.

Reporting a conflict is **always** correct. Guessing is **never** correct.

---

## 8. Teaching Mode

The project owner is a mid-level .NET developer working toward senior. **Every non-trivial change must teach.**

When you write code, explain — **in the conversation and the pull request description**, not as code comments:

- **Why** this approach, and what problem it actually solves
- **Which alternative you rejected**, and the trade-off that decided it
- **What a senior engineer would worry about here** (failure mode, race, N+1, lifetime bug, cache invalidation, security boundary)
- **What to measure** to know whether it worked

**Do not clutter the code to teach.** `12` is explicit: comments state constraints the code cannot show — not narration, not tutorials. Teach in the explanation; keep the code clean.

Concepts worth pausing on when they arise: EF Core query plans and indexing · service lifetimes · rotating refresh tokens and reuse detection · cache invalidation · offline sync conflict resolution · observability (and actually *using* it to diagnose something).

---

## 9. Quick Reference

| Need | Go to |
|---|---|
| Where does this file go? | `06-monorepo-structure.md` |
| What is the API contract? | `08-api-standards.md` |
| What are the design values? | `docs/design-system/design-tokens.md` |
| What must a Topic contain? | `10-content-architecture.md` |
| Is this in scope? | `04-development-roadmap.md` |
| Was this decided already? | `docs/adr/` |
| Am I done? | `12` (build) **and** `13` (verify) |
