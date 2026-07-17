# ADR-0028 — Publishing is one recorded human act, not four

- **Status:** Proposed
- **Date:** 2026-07-17
- **Deciders:** Tolga Yazgı (owner)
- **Supersedes:** CLAUDE.md §4 ("transitioning `AiDraft → Published`" as a forbidden action) — see Decision 4
- **Related:** CLAUDE.md §1.5, ADR-0010 (AI scope), ADR-0024 (block-based content), `10-content-architecture.md`
  (Topic Lifecycle), `14-agent-ecosystem.md`

---

## Context

`10`'s Topic Lifecycle has seven stages:

```
Idea → Outline → AiDraft → TechnicalReview → EditorialReview → Approved → Published
```

That ladder models a **team**: an author writes, a technical reviewer checks the engineering, an editor
checks the writing, someone approves, someone publishes. Each stage is a different person applying a
different lens, and the row in `TopicReviews` is the trace of who did what.

WhyStack has one person. Tolga writes the content, and Tolga is the only account with any authoring role.
Walking a topic through four transitions alone is one person clicking a button four times, producing four
audit rows that all say the same thing about the same act. That is ceremony, and ceremony that costs a step
per topic across a corpus is a tax on the thing the product needs most: more content.

**Two facts make this worth deciding now rather than later.**

First, the gate is currently impassable. `TransitionTopicHandler` validates a `TopicDraft`, and `TopicDraft`
carries `Sections` only. ADR-0024 replaced sections with blocks; `TopicSections` has zero rows. So a finished
block-based topic — all four mandatory beats present — is refused with twelve errors naming section types the
model retired and the studio has no boxes for. Nothing can be published at all. That is a defect, and it is
fixed regardless of this ADR (see Decision 1).

Second, the rule this ADR touches is written as a hard stop:

> **CLAUDE.md §1.5** — AI-generated content never publishes without human review. **No exceptions.**
>
> **CLAUDE.md §4** — Forbidden: publishing content that skipped review · transitioning `AiDraft → Published`
>
> **ADR-0010** — AI is used exclusively inside the content pipeline, under founder control, **with mandatory
> human review.**

## What the rule is actually protecting

The rule and the ladder are not the same thing.

**The rule** is: a human reads the content before a reader does. It exists because the drafts are
AI-generated — the status is literally named `AiDraft`, and the content files being authored today
(`whystack-dil-runtime-dilin-temelleri-junior.md`) are model output. Pasting an AI draft is not reading it.
The first two topics this project ever published went out with no Checkpoint at all, which is what that rule
looks like when it is documented and not enforced.

**The ladder** is one implementation of that rule — the implementation a team of four needs. It is not the
rule.

So the question this ADR answers is not "should content be reviewed?" It is "how many recorded acts does one
person's review take?" The answer this proposes is one. Not zero.

## Decision

1. **The gate reads BLOCKS, not sections.** `TransitionTopicHandler` validates the four mandatory beats
   (ADR-0024: Hook, Checkpoint, Summary, Next) via `BlockSkeletons.MissingMandatory` — the function that has
   always known the rule and has never been called by the code path that enforces it. The twelve mandatory
   `SectionType` rules stop being applied to topics that have no sections.

   **This is not part of the trade-off below.** It is ADR-0024 being implemented in a place that was missed,
   and it happens whether or not the rest of this ADR is accepted.

2. **An author may publish their own topic in one transition: `AiDraft → Published`.**

   The transition is refused unless the topic passes the beats gate. A topic with no Checkpoint is a topic
   nobody can ever finish — completion is a correct Checkpoint answer — so the basamak would never fill and no
   station would ever go gold, with nothing on any screen to explain why.

3. **The act is recorded, and it means something.** The transition writes one `TopicReview` row whose note is
   not optional: by publishing, the author states they have READ this topic, not merely written or pasted it.
   `TopicReviews` remains the trace of a human decision — which is why its foreign key is `NO_ACTION` while
   every other one cascades, and why deleting 1,432 user accounts left it untouched.

   One row, not four. The count changes; the meaning does not.

4. **CLAUDE.md is amended, not worked around.** §4's "transitioning `AiDraft → Published`" is removed as a
   forbidden action and replaced with: *"publishing a topic that has not passed the mandatory-beats gate"* and
   *"publishing on a human's behalf — an agent may never transition a topic to Published."*

   §1.5 stands unchanged and is satisfied: a human reads and affirms before a reader sees it.

5. **The intermediate stages stay in the model.** `TechnicalReview`, `EditorialReview` and `Approved` remain
   valid statuses and remain reachable. Nothing is deleted. The day a second editor exists, the ladder is
   there — this ADR removes the requirement to climb it alone, not the ladder.

6. **An agent still may not publish.** CLAUDE.md §6 is untouched: agents propose, humans approve. Claude Code
   may draft, validate and refuse — it may not transition a topic to `Published`, and no automation may.

## What this costs — read this before accepting

**The one-pass problem.** Writing and reviewing are different mental states, and the ladder's real value to a
solo author was never the four clicks — it was the gap between them. You write on Tuesday and review on
Thursday, and on Thursday you see what you could not see on Tuesday. Collapsing the ladder makes it
frictionless to write and publish in one breath, and the errors that survive are exactly the ones a second
look catches. This ADR does not prevent that; it makes it your discipline instead of the system's.

**The audit trail loses granularity.** "Technically reviewed by X on the 4th, editorially by Y on the 6th"
becomes "published by Tolga on the 4th". For one person that distinction was fiction anyway. For two people it
is not, and this is the thing to revisit the day someone else writes a topic.

**The gate is now load-bearing.** With three stages removed, the beats check is the only thing between a draft
and a reader. It must be enforced by the transition — not by the "Doğrula" button, which is advisory and
skippable, and which is how this project already shipped two topics with no Checkpoint.

## Alternatives rejected

**Keep the ladder.** Correct in principle, ceremony in practice for a team of one, and the friction is paid
per topic forever on the thing the product most needs more of.

**Skip review entirely — publish on save.** This is what "gerek yok" would mean taken literally, and it is
refused. The drafts are AI output. A save is a keystroke; publishing has to be a decision, and a decision has
to be an act somebody took on purpose.

**Auto-advance through the stages on one click.** Four audit rows written by one click is a trail that lies —
it records reviews that did not happen. One honest row beats four fictional ones.

## Consequences

- `TransitionTopicHandler` validates blocks; the section rules apply only to topics that have sections (none).
- `ContentLifecycle.MayTransition` permits `AiDraft → Published`; every other skip stays forbidden.
- The studio's publish panel becomes one action, and it states what publishing asserts.
- `PublishGateTests` inverts: it currently documents the defect, and will document the rule.
- The dead section model (30 files, 0 rows) loses its last caller and can be removed in its own change.
