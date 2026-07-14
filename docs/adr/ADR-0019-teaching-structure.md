# ADR-0019 — Teaching Structure

- Status: Accepted
- Date: 2026-07-14
- Owner: WhyStack Core Team
- Sources: `10-content-architecture.md` § Writing Philosophy, § Content Quality Standards; `02-product-principles.md`; ADR-0018

---

## Context

`10` § Writing Philosophy is explicit about what to avoid and what to prefer:

> **Avoid:** unnecessary storytelling · filler text · repetitive explanations
> **Prefer:** clarity · precision · **progressive explanation** · real engineering examples · comparisons

And `10` § Content Quality Standards lists **Progressive** as a quality every topic must have.

**It never says what "progressive" means.** The first two topics were written into that gap, and they came
back rejected by the reviewer with three findings — all correct:

1. **Storytelling, not teaching.** Each section was a self-contained essay ending in a flourish. `10`
   forbids this by name.
2. **No progression.** The sections did not build. A reader was *told* things; they never met a problem,
   never watched code fail, never derived a concept from a consequence.
3. **The Turkish was a translation, not Turkish.** English sentence rhythm forced into Turkish words —
   grammatically fine, tonally pretentious, and unpleasant to read.

## Decision

**1. Progressive explanation means PROBLEM FIRST.**

A topic derives its concept from a failure the reader can feel, in this order:

```
The code you would write        →  a real, plausible attempt
What actually happens           →  it fails, and the failure is shown, not described
Why                             →  the mechanism, revealed by the failure
The concept                     →  named only now, because now it has somewhere to land
What it costs                   →  the trade-off, because nothing is free
```

The concept is **named after** the reader needs it, not before. `10`'s mandatory sections stay exactly as
they are — this is the *weave through* them, not a replacement for them:

| Section | Carries |
|---|---|
| `WhyItExists`, `ProblemItSolves` | the failing code and its consequence |
| `CoreMentalModel` | the mechanism the failure exposed |
| `CoreConcepts` | the concept, named |
| `BasicExample`, `RealWorldScenario` | the concept used, then used at scale |
| `CommonMistakes`, `TradeOffs` | what it costs, and how people still get it wrong |

**2. Turkish is WRITTEN, not translated.**

English stays canonical (ADR-0018: it is the source of meaning, and a translation with no source cannot
be reviewed). But `tr.md` is **written in Turkish** — same concept, same example, same terminology, native
sentence rhythm. A sentence-for-sentence translation of English prose produces Turkish nobody would
choose to read, and the terminology gate already guarantees the one thing a loose translation could
break: **the technical terms survive verbatim.**

**3. A table holds facts, not paragraphs.**

`10` requires tables. It does not require prose in them. A two-column table whose cells are full sentences
is a comparison nobody can see and a page that is painful on a phone. Table cells are **short** — the
content validator enforces it, so this cannot come back.

## Alternatives Considered

- **Mental model first, then code.** Rejected: safer and more passive. A reader who is never wrong never
  has to change their mind, and changing your mind is what learning is. It stays available for topics
  where there is no failure to show (a `Concept` topic about what IL *is*, for instance).

- **Layered depth (Junior / Mid / Senior in one topic).** Rejected for MVP: it means writing every topic
  three times, and `07` already models level as a property of the TOPIC. Two topics at two levels is
  cheaper than one topic with three voices, and it is what the roadmap already assumes.

- **Keep the literal translation.** Rejected: it produced exactly the text the reviewer would not read
  twice, which is the only test that matters for a reading product.

## Consequences

- The two shipped topics are **rewritten**. They were `AiDraft`; nothing was published, and the gate did
  its job — a reviewer read them and refused them, which is CLAUDE.md §1.5 working exactly as designed.
- `tests/content-validation` gains a rule: a table cell longer than a short phrase fails the build.
- The Markdown renderer's table gets zebra rows and wider cells — but the rule above is the real fix.
  Rendering cannot rescue a table that should have been a paragraph.
- `10` § Writing Philosophy gains a pointer to this ADR. It stays the owner; this defines the one word it
  left undefined.

## References

- `10` § Writing Philosophy, § Content Quality Standards, § Topic Blueprint
- ADR-0018 (Content Ingestion), CLAUDE.md §1.5
