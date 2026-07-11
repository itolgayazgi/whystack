# Licensing

This repository holds **two different assets, and they are licensed differently** (ADR-0014).
A single licence would either over-protect the code or under-protect the content.

| Asset | Licence | File |
|---|---|---|
| **Code** — `apps/`, `packages/`, `scripts/`, `infrastructure/`, `tests/`, configuration | **MIT** | [`LICENSE`](LICENSE) |
| **Content** — everything under `content/` | **CC BY-SA 4.0** | [`content/LICENSE`](content/LICENSE) |
| **Brand** — the name *WhyStack*, the logo, the brand identity | **All rights reserved** | this file |

> `LICENSE` is kept as the unmodified MIT text so that automated licence detection
> (GitHub, SPDX scanners, dependency tooling) identifies it correctly. The scope and
> trademark terms live here rather than appended to it — appending prose to a licence
> file breaks that detection and makes the repository read as *unlicensed*, which is the
> opposite of the intent.

---

## Code — MIT

The code is **not the moat**. A modular monolith and a React Native client are reproducible
by anyone. Product Principle 12 requires this repository to *teach* — architecture, naming,
testing strategy — and teaching requires it to be open and readable.

Use it, fork it, ship it. Attribution per the MIT terms is all that is asked.

## Content — CC BY-SA 4.0

Everything under `content/` — topics, roadmaps, quizzes, terminology, examples, diagrams —
is the **product**, and it is the asset worth protecting.

**You may:** share it, adapt it, translate it, and use it commercially — including as
training data for machine learning models. That permission is deliberate and consistent
with ADR-0011.

**You must:** give attribution, and license your derivative under the same terms.

Share-alike is the point. A competitor may republish this corpus, but their version must
also be open — which removes most of the commercial incentive to do so. **The content
cannot be taken closed.**

## Brand and trademark — all rights reserved

The name **WhyStack**, the WhyStack logo and the WhyStack brand identity are **not**
covered by MIT or by CC BY-SA. All rights in them are reserved.

This is the actual protection, and it is separate from copyright.

Under CC BY-SA you may republish the content with attribution. You may **not**:

- call the result *WhyStack*,
- use the name or logo in a way that suggests affiliation with or endorsement by this
  project, or
- present a derivative as if it were this project.

## Third-party material

Educational content does **not** reproduce third-party documentation verbatim (Microsoft
Learn, framework documentation, blog posts). External sources are referenced and cited;
prose and code examples are original. This is a copyright obligation, and it is part of the
content review checklist — not only a line in a licence.

## Contributions

By contributing you agree that code contributions are licensed under **MIT** and content
contributions under **CC BY-SA 4.0**.

There is no Contributor License Agreement today. One may be introduced before community
contribution opens (ADR-0014, Deferred).
