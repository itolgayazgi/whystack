# ADR-0014 — Licensing

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Sources: Delegated decision (2026-07-11); `06-monorepo-structure.md` (LICENSE), ADR-0011 (AI crawler policy), ADR-0012 (no MVP monetization)

---

## Context

`06-monorepo-structure.md` left the licence open: *"The exact license should be selected before public contribution begins."* The repository will be public from the first commit, so the licence must be settled now — retroactively changing a licence after contributions arrive is painful and, without a CLA, sometimes impossible.

The critical observation is that **this repository contains two different assets, and they must not share one licence**:

| Asset | Value |
|---|---|
| **Code** (`apps/`, `packages/`, `scripts/`, `infrastructure/`) | **Not the moat.** A modular monolith and a React Native client are reproducible by anyone. Principle 12 explicitly wants the repository to be an *educational example*, which requires it to be readable and open. |
| **Content** (`content/`) | **The product.** This is the asset worth protecting. |

A single licence file would either over-protect the code or under-protect the content.

## Decision

### 1. Code → **MIT**

Applies to `apps/`, `packages/`, `scripts/`, `infrastructure/`, `tests/` and configuration.

**Rationale:** the code is not the differentiator; protecting it buys nothing. `02` Principle 12 requires the repository to teach ("Developers exploring the repository should learn: Architecture, Clean Code, Naming, Testing Strategy…") — that requires openness. MIT is the simplest, most recognised, lowest-friction option, and it maximises the repository's value as a portfolio and teaching artefact.

### 2. Content → **CC BY-SA 4.0**

Applies to everything under `content/` — topics, roadmaps, quizzes, terminology, examples, diagrams.

**Rationale, by elimination:**

| Considered | Verdict |
|---|---|
| **CC BY-NC** (non-commercial) | **Rejected.** Contradicts two existing decisions: AI training crawlers are allowed (ADR-0011) and AI training is commercial; and B2B / enterprise onboarding is the primary future revenue path (`01`), which NC would block. "Commercial" is also legally ambiguous. |
| **CC BY-ND** (no derivatives) | **Rejected.** Kills translation and community contribution — Sprint 13 becomes impossible. |
| **Proprietary / all rights reserved** | **Rejected.** Forces a CLA before any contribution, and is inconsistent with the open crawler policy. |
| **CC BY** (attribution only) | Viable, but leaves the corpus open to being absorbed into a **closed, proprietary** competitor. |
| **CC BY-SA** ✅ | **Selected.** |

**What CC BY-SA buys:**

- ✅ Community contribution and translation are permitted (Sprint 13 works).
- ✅ AI training is permitted — consistent with ADR-0011.
- ✅ **Attribution is mandatory** — which is precisely the brand and citation outcome the project wants.
- ✅ **Share-alike prevents a proprietary clone.** A competitor may republish, but their derivative must also be open — which removes most of the commercial incentive to do so.
- ✅ **It does not constrain WhyStack itself.** Copyright remains with the project; share-alike binds *others'* redistribution. Hosting, Premium AI, offline packs and B2B portals remain fully monetizable (ADR-0012's future paths are unaffected).

### 3. Brand and trademark → **All rights reserved**

The **product name, logo and brand are not covered by either licence.**

This is the actual protection, and it is separate from copyright. Under CC BY-SA someone may republish the content with attribution — but they **may not call it by the product's name.** This must be stated explicitly in `LICENSE` and `README.md`.

### 4. File layout

```
LICENSE              → MIT (code) — unmodified licence text, nothing appended
content/LICENSE      → CC BY-SA 4.0 (educational content)
LICENSING.md         → how the two licences fit together, plus the trademark reservation
README.md            → summary table, linking to LICENSING.md
```

> **Amendment (2026-07-11, approved by the project owner).** This section originally placed the
> scope and trademark terms *inside* `LICENSE`. That was implemented, pushed, and **failed in
> practice**: GitHub's licence detector (`licensee`) matches a licence file against known licence
> texts, and the appended prose broke the match. The repository reported
> `"license": "NOASSERTION"` — it read as **unlicensed**, which is the exact opposite of this
> ADR's intent.
>
> `LICENSE` is therefore now the unmodified MIT text, and the scope and trademark terms live in
> `LICENSING.md`. **The decision is unchanged** — code is MIT, content is CC BY-SA 4.0, the brand
> is reserved. Only the file that carries the trademark sentence moved. Trademark rights arise from
> the terms themselves, not from which file states them.

### 5. Third-party content

Educational content must **not reproduce third-party documentation verbatim** (Microsoft Learn, framework docs, blogs). Reference and cite; write original prose; author original code examples. This is a copyright obligation and belongs in the content pipeline's review checklist (`10`, `13`), not only in the licence.

## Deferred

**Contributor License Agreement (CLA).** Under CC BY-SA, contributions arrive share-alike; relicensing the corpus later would require contributors' permission. A CLA or DCO resolves this — but there are **no contributors yet**, and adding one now is friction with no benefit. **Revisit in Sprint 13**, before community contribution opens.

## Consequences

- The repository can go public with a settled, internally consistent licensing position.
- ADR-0011 (allow all crawlers) and this ADR are now **mutually consistent** — a permissive content licence and open crawling are the same posture.
- `06-monorepo-structure.md`'s LICENSE section is updated (Phase 4 patch).
- **Note:** the trademark clause depends on the final product name, which is under review. The clause is name-agnostic and requires no change if the name changes.

## References

- ADR-0011 (Discoverability & AI Crawler Policy), ADR-0012 (Monetization Deferral)
- `01`, `02` (Principle 12), `06`, `10`, `13`
