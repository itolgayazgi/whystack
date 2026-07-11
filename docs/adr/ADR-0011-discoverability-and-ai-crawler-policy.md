# ADR-0011 — Discoverability and AI Crawler Policy

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Extends: ADR-0009 (Public Content SEO Surface)
- Sources: Confirmed decision (2026-07-11); `10-content-architecture.md`, `06-monorepo-structure.md` (LICENSE)

---

## Context

ADR-0009 establishes an indexable static surface. Two questions remained:

1. **Which search engines** does the surface target, and does each need separate work?
2. **How should WhyStack behave toward AI systems** — both those that *cite* sources (ChatGPT Search, Perplexity, AI Overviews) and those that *train* on content (GPTBot, Common Crawl, Google-Extended)?

The second is a strategic decision, not a technical one, and it interacts with the still-open LICENSE decision.

## Decision

### 1. Search engines — one implementation, no separate architecture

Technical SEO is ~95% shared across engines. **No engine-specific architecture is created.** The only additions are operational:

- Register in **Bing Webmaster Tools** and **Yandex Webmaster**; submit the sitemap generated from content metadata.
- Implement **IndexNow** — a single HTTP POST on content publish that notifies **Bing and Yandex** immediately. (Google does not support it and is unaffected.) This is added to the content publishing pipeline.
- Priority: **Google** (primary), **Bing** (secondary — it also feeds DuckDuckGo/Yahoo and parts of AI search), **Yandex** (low — negligible for the Turkish and global developer audience).

### 2. AI crawler policy — allow all

> **Revised 2026-07-11.** This ADR originally allowed citation crawlers and blocked training crawlers. That policy was calibrated for an established *publisher* with negotiating leverage. WhyStack is an unknown, pre-launch, **free** educational site whose primary problem is that nobody knows it exists. The block bought nothing and cost real long-term presence. The decision is reversed; the original reasoning is preserved under *Alternatives Considered*.

`robots.txt` on the public surface **allows all well-behaved crawlers**:

| Crawler | Purpose | Policy |
|---|---|---|
| `Googlebot`, `Bingbot`, `YandexBot` | Search indexing | **Allow** |
| `OAI-SearchBot`, `ChatGPT-User` | ChatGPT Search / user-triggered fetch (citation) | **Allow** |
| `PerplexityBot` | Perplexity (citation) | **Allow** |
| `ClaudeBot` | Claude web access (citation) | **Allow** |
| `GPTBot` | OpenAI model training | **Allow** |
| `CCBot` | Common Crawl (feeds many models' training) | **Allow** |
| `Google-Extended` | Gemini training | **Allow** |
| `Applebot-Extended` | Apple model training | **Allow** |

Non-public paths (drafts, admin, editorial) remain disallowed. `robots.txt` is a **generated artifact** of the SSG build.

**Rationale:**

- **Discoverability outweighs content protection at this stage.** Presence in training data means a model can name WhyStack *from memory* when a developer asks where to learn something — permanent, free brand presence that citation crawlers alone do not provide.
- **There is nothing to protect.** The content is free, there is no revenue (ADR-0012), and a pre-launch site has zero licensing leverage.
- **The moat is not the prose.** Models already know what Dependency Injection is, from Microsoft Learn, Stack Overflow and thousands of blogs — WhyStack is not the marginal source of that fact. What WhyStack adds — curation, sequence, roadmaps, trust, offline packs, Developer Lab tools, progress tracking — **cannot be trained into a model.** The commodity knowledge may be absorbed; the moat cannot.
- **`robots.txt` is voluntary compliance anyway.** It never stopped bad actors, only well-behaved bots.

**Accepted costs:**

| Cost | Magnitude |
|---|---|
| Content cannibalisation (an AI answers instead of sending a visit) | Real but small — this already occurs via the citation crawlers, and the knowledge absorbed is commodity |
| Crawler bandwidth | Negligible — the public surface is static HTML behind a CDN |
| Forgone future licensing leverage | Theoretical; requires a scale WhyStack does not have |
| Partial irreversibility | Content already crawled cannot be un-trained. Future content can still be blocked if leverage ever emerges. |

**Revisit when:** WhyStack has meaningful scale and brand, such that licensing leverage becomes a real concept rather than a lottery ticket.

**LICENSE interaction:** allowing all crawlers is **consistent with a permissive content licence** (e.g. CC-BY), which also supports the future community-contribution goal (Sprint 13). The two decisions should be finalised together.

### 3. Generative Engine Optimization (GEO)

GEO is largely **downstream of SEO** — retrieval-based AI systems use search infrastructure, so ranking well is the main lever. Beyond that, the following are adopted because they are cheap and fit the existing architecture:

- **Section-structured content is retained as a GEO asset.** The doc-10 blueprint (`Definition`, `Why It Exists`, `Problem It Solves`, `Trade-Offs`, `Common Mistakes`, `Version Notes`) produces **self-contained, quotable chunks**, which is what RAG systems retrieve and cite. This is an existing advantage, not new work.
- **Serve a clean Markdown representation of each public page** (e.g. `/learn/{tech}/{version}/{slug}.md`). Content is already canonical Markdown; exposing it gives AI crawlers a parse-clean source with no HTML noise. Low cost, natural fit.
- **Stable section anchors** (`#why-it-exists`) so AI systems can deep-link to the exact passage cited.
- **Version-explicit, dated claims** (".NET 8 behaves as X") are inherently more citable than vague prose — already mandated by `10` and `13`.
- **JSON-LD** (`Article`, `FAQPage`, `Course`, `BreadcrumbList`) — already required by ADR-0009; serves both SEO and GEO.
- **`llms.txt`** — published as a cheap, optional signal. **Its effectiveness is unproven**; no further investment is made on this basis.

**No further GEO investment.** The field has no reliable playbook; the 80/20 is good SEO plus well-structured, specific content, both of which are already required.

### 4. Accepted trade-off

An AI that answers a user's question by citing WhyStack may mean the user never visits. **GEO yields authority and brand, not guaranteed sessions.**

This is accepted because the product is resilient to it: an LLM can reproduce "what is DI," but it cannot provide the sequenced roadmap, progress tracking, quizzes, offline Knowledge Packs, or the daily-use Developer Lab tools. AI citation therefore drives users toward exactly what WhyStack offers and the LLM does not.

## Alternatives Considered

- **Allow citation crawlers, block training crawlers** (`GPTBot`, `CCBot`, `Google-Extended`). **This was the original decision, now reversed.** The reasoning — retain the content corpus and preserve future licensing leverage — is sound **for an established publisher**. It does not survive contact with WhyStack's actual situation: no brand, no revenue, no leverage, free content, and a primary problem of obscurity rather than theft. The block would have forfeited permanent presence inside models in exchange for protecting an asset that is neither scarce nor monetised.
- **Block all AI crawlers.** Rejected: forfeits discoverability in an increasingly AI-mediated research workflow, with no offsetting benefit for free educational content.
- **Engine-specific SEO tracks.** Rejected: no technical basis; the work is shared across engines.

## Consequences

- `robots.txt` is a **generated artifact** of the SSG build, not a hand-edited file.
- IndexNow ping is added to the content publishing pipeline (`scripts/content/`).
- A Markdown representation endpoint/route is added to the SSG output.
- Bing/Yandex webmaster registration and sitemap submission become release-checklist items (`13`).
- LICENSE selection must remain consistent with the crawler policy (`06`).

## References

- ADR-0009 (Public Content SEO Surface)
- ADR-0004 (Knowledge Graph — internal linking)
- `06` (LICENSE), `10`, `13`
