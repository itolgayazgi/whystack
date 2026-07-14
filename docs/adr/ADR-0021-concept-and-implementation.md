# ADR-0021 — A Topic Is A Concept; Implementations Are Per-Ecosystem

- Status: Accepted
- Date: 2026-07-14
- Owner: WhyStack Core Team
- Sources: `01-product-vision.md`; `10-content-architecture.md` § Master Topic Structure; ADR-0002 (Canonical Topic Model); ADR-0004 (Knowledge Graph)

---

## Context

The product's promise, in its own words on the splash screen:

> **Nasıl'dan önce, neden.** — *Why before how.*
> *Teknolojileri var olma sebepleriyle öğren.* — *Learn technologies by the reasons they exist.*

Connection Pooling exists for a reason that has nothing to do with C#. A pool is a bounded set of expensive
things you reuse instead of recreating; too small and requests queue, too large and the database drowns.
That is true in .NET, in Java, in Node and in PHP, and it will be true in whatever comes next.

**The current model cannot say that.** ADR-0002's Topic has one `Technology`. So `Connection Pooling` must
be written once for C#, once for Java, once for Node — three topics, three copies of the same reasoning,
drifting apart from the day they are written. The *reason* — the thing this product exists to teach — is
duplicated, and the *implementation* — the thing that genuinely differs — is buried inside each copy.

That is backwards. It also makes the ecosystem the reader chose in onboarding meaningless: they picked
`.NET`, and the product answers by hiding the other two-thirds of the corpus from them.

## Decision

**1. A topic is a CONCEPT. It belongs to a domain (Backend, Database, …), not to a language.**

Everything that explains *why* is written once and is ecosystem-independent:

| Section | Ecosystem-independent |
|---|---|
| `WhyItExists`, `ProblemItSolves` | ✅ the problem is not a language feature |
| `CoreMentalModel`, `CoreConcepts` | ✅ |
| `TradeOffs` | ✅ a pool is a guess in every runtime |
| `CommonMistakes` | ✅ mostly |

**2. Implementation is a per-ecosystem section of the same topic.**

`Syntax`, `BasicExample`, `ProgressiveExamples`, `InternalMechanics` and `VersionNotes` are written **per
ecosystem**, and the reader switches between them:

```
Connection Pooling                        (Backend · concept)

  Why it exists          ← written once
  The problem it solves  ← written once
  Trade-offs             ← written once

  ▾ IMPLEMENTATION   [ .NET ▾ ]           ← the reader chooses
      Npgsql manages the pool from the connection string…
      → Java:    HikariCP
      → Node.js: pg-pool
```

**3. An ecosystem the reader has not selected is not deleted — it is not shown.** The concept is the same
page for everyone. Only the implementation panel changes. A reader who wants to see how Java does it can
switch, and that is a *feature*: the whole point of teaching the reason first is that the reason
transfers.

**4. A topic may have NO implementation.** "What is a transaction?" is a concept with no code. The panel
simply does not render, exactly as a graph-derived section with no edges does not render.

**5. Ecosystem and programming language are separate.** `.NET` is an ecosystem; `C#` is a language within
it. `10`'s Technology Hierarchy already draws this line — this decision makes the database draw it too.

## Alternatives Considered

- **One topic per technology (the current model).** Rejected: it duplicates the reasoning, which is the
  product's entire value, and buries the difference, which is the only thing that actually differs. It also
  makes the reader's ecosystem choice a filter that hides content rather than a lens that focuses it.

- **A shared "concept" topic that language topics LINK to.** Rejected: it is the same duplication with an
  extra click. A reader on `csharp.connection-pooling` would have to leave the page to learn why pooling
  exists, and nobody does.

- **Implementation as a separate entity with its own lifecycle.** Deferred, not rejected. It is the right
  answer the day a Java implementation is reviewed by someone who does not review the C# one. Today there
  is one editor; a per-implementation review state would be ceremony with nobody to perform it.

## Consequences

- **ADR-0002 is amended, not superseded.** `10` remains the Single Source of Truth for the Topic model and
  the section list. What changes is that a section may be written **once** or **per ecosystem**, and `10`
  gains that column.
- `07` gains `Ecosystems`, `ProgrammingLanguages` and `TopicImplementations`. `Topics.Technology` becomes
  `Topics.DomainId` — a topic belongs to Backend, not to C#.
- The Knowledge Graph (ADR-0004) is **unchanged**, and this is the point: `Connection Pooling` requires
  `Threads and Task` regardless of which language you write either one in. The graph was always about
  concepts; the model just did not know it.
- Onboarding's ecosystem choice becomes a real preference: it selects the default implementation panel, not
  a filter over the corpus.
- The two shipped topics (`what-is-csharp`, `variables-and-data-types`) are **language topics, correctly**
  — C# *is* the subject, not the implementation. They stay as they are. The distinction matters: a topic
  about a language is not the same as a concept implemented in one.

## References

- `01` (product vision), `10` § Technology Hierarchy, § Master Topic Structure
- ADR-0002 (amended), ADR-0004 (unchanged), ADR-0020 (source of truth)
