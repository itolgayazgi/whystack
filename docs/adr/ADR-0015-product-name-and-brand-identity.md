# ADR-0015 — Product Name and Brand Identity

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Supersedes: the previous product name (`CodeTecho`) and repository name (`Learn-Code-Tech`)
- Sources: Confirmed decision (2026-07-11); `01-product-vision.md`, `03-philosophy.md`, ADR-0009, ADR-0014

---

## Context

The product was named **CodeTecho**, with a separate repository name (`Learn-Code-Tech`). Two problems emerged.

**1. The name did not survive word-of-mouth.** The product's growth thesis is a person saying *"have you seen this?"* — the name is transmitted **verbally**, then re-typed. "CodeTecho" is heard as "CodeTech", and that namespace is heavily occupied: Codetechie, Code Techies, CodeTech (agency), CodeTech Inc., codetech.one, "Code, Tech and Tutorials". A listener searching the name lands on someone else. "Techo" also carries no meaning and reads as a misspelling of "Techno".

**2. It said nothing about the product.** The name is the first and cheapest piece of positioning, and it was spending that opportunity on nothing.

A rejected alternative direction is worth recording: adding **"Tutorial"** to the name was considered and **refused**, because it contradicts the product's core positioning. `00` states the product exists *"rather than becoming another tutorial website"*; `01` states it *"does not compete by publishing more tutorials"*; `10` Forbidden Pattern 01 forbids *"Syntax Without Context"*. Naming the product after the thing it was built not to be would have been self-defeating.

**Timing:** no code exists yet. The rename is a documentation find-and-replace now; after Sprint 1 it becomes .NET namespaces, package scopes, git history, domains and store listings.

## Decision

### 1. Product name — **WhyStack**

The name states the product's thesis: the ***why*** behind the ***stack***. It maps directly onto the content blueprint, whose first two sections are literally *"Why It Exists"* and *"Problem It Solves"* (`10`).

**Properties:**
- **Distinctive** — no competing product found; a coined name is the strongest form of trademark.
- **Meaningful** — it declares the positioning instead of wasting it.
- **Survives speech** — short, unambiguous, pronounceable in both Turkish and English.
- **Consistent with the philosophy** — `03`: *"We do not teach technologies. We teach the way engineers think."*

### 2. Repository name — **`whystack`**

The previous split (product `CodeTecho` / repository `Learn-Code-Tech`) is **collapsed**. One name, one brand. A repository name that differs from the product fragments recognition for no benefit.

### 3. Domains

| Domain | Role |
|---|---|
| **`whystack.dev`** | **Canonical.** Public site and brand. `.dev` signals a developer product without spending the name on the word "Dev", and is HTTPS-enforced. |
| `api.whystack.dev` | API |
| `docs.whystack.dev` | Problem Details error type URIs; API migration links |
| `download.whystack.dev` | Knowledge Pack distribution |
| `whystack.online`, `whystack.org` | **Defensive holdings.** They **redirect** to `whystack.dev`. |

**`.online` and `.org` must not serve content.** Split canonical domains fragment SEO authority and duplicate content — the exact opposite of the goal in ADR-0009. They exist to deny the names to others, nothing more.

### 4. Naming conventions

| Context | Form |
|---|---|
| Product, .NET namespaces, docs, UI | `WhyStack` (`WhyStack.Api`, `WhyStack.Domain`) |
| Repository, package scope, URLs | `whystack` (`@whystack/ui`) |
| Knowledge Pack file extension | `.wspack` |

### 5. Trademark

Per ADR-0014, the **name, logo and brand are all rights reserved** and are **not** covered by the MIT (code) or CC BY-SA (content) licences. Content may be reused with attribution; **it may not be published under the WhyStack name.**

## Alternatives Considered

Availability was checked; each of the following is **already in use**:

| Candidate | Status |
|---|---|
| `DevAtlas` | Taken — active developer productivity app |
| `DevContext` | Taken — MCP server, VS Code extension, npm package |
| `DevDepth` | Taken — publishes developer roadmap guides (directly competing space) |
| `Underhood` | Taken — `underhood.dev`, `underhood.blog` |
| `DevPath` | Taken — developer learning platform |
| `DevGraph` | Taken — company and open-source project |
| `DeepDev` | Taken |
| `DevReason` | **Available.** Rejected: contains "Dev" as requested, but is corporate, flat, and far less memorable than WhyStack. |
| Anything containing **"Tutorial"** | **Refused on positioning grounds** (see Context). |

## Consequences

- Every occurrence of `CodeTecho` / `codetecho` / `Learn-Code-Tech` is replaced across the Foundation Pack, ADRs, `README.md`, `CLAUDE.md` and the design-token specification (321 occurrences, 31 files).
- .NET namespaces become `WhyStack.*`; package scope becomes `@whystack/*`; domains as above.
- **Outstanding owner actions** (cannot be verified from documentation):
  1. **Register the domains** — `whystack.dev` (primary), `whystack.online`, `whystack.org`.
  2. **Search the trademark registries** — TÜRKPATENT, EUIPO, USPTO. A web search cannot see a registered mark that has no website; this is the one remaining real risk and it must be checked before launch.
  3. Secure the GitHub organisation / repository name and social handles.

## References

- ADR-0009 (SEO surface — canonical domain matters), ADR-0014 (Licensing — trademark reservation)
- `00`, `01`, `03`, `06`, `10`
