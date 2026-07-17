# ADR-0028 — The content is human-written, and publishing is one act

- **Status:** Proposed
- **Date:** 2026-07-17
- **Deciders:** Tolga Yazgı (owner)
- **Supersedes:** ADR-0010 Decision 1 ("MVP AI scope = content production only"); CLAUDE.md §4's
  "transitioning `AiDraft → Published`" as a forbidden action
- **Related:** CLAUDE.md §1.5, ADR-0003, ADR-0024, `10-content-architecture.md` (Topic Lifecycle),
  `11-ai-content-pipeline.md`, `14-agent-ecosystem.md`

---

## Context

An AI agent wrote WhyStack's topics. The owner read them and judged the writing bad, and decided to write the
content himself. That is not a hypothesis in this document — it is what happened, and it is the reason this
ADR exists.

Everything below follows from that one fact, and the reason it needs an ADR rather than a commit is that the
fact quietly invalidates a chain of accepted decisions.

**ADR-0010 Decision 1:**

> **MVP AI scope = content production only.** AI is used exclusively inside the `11-ai-content-pipeline.md`
> workflow, under founder control, with mandatory human review.

Content production was not *part* of the MVP's AI scope. It **was** the scope — Decision 3 had already removed
the runtime assistant. So if AI does not produce content, ADR-0010 has no subject left.

**CLAUDE.md §1.5:**

> **AI-generated content** never publishes without human review. No exceptions.

Read the sentence. The rule is scoped to AI-generated content — that is its subject, not an example. The
seven-stage lifecycle (`Idea → Outline → AiDraft → TechnicalReview → EditorialReview → Approved → Published`)
is that rule's machinery: a human reads what a model wrote before a learner does. The status in the middle of
it is named `AiDraft` because that is literally what it held.

When the author writes the topic, there is no model output to catch. The rule does not need an exception. **It
has no subject.**

## Decision

1. **AI does not produce WhyStack's content.** Topics are written by a human. This supersedes ADR-0010
   Decision 1.

2. **The MVP product ships with no AI feature at all.** ADR-0010 Decision 3 removed the runtime assistant;
   Decision 1 was the only AI left, and Decision 1 above removes it. Runtime AI cost in MVP remains $0, and now
   authoring AI cost is $0 too.

3. **This is about the PRODUCT, not the workshop.** `14-agent-ecosystem.md` and Claude Code are unaffected:
   engineering agents still write code, review it and refuse to publish. The distinction is who the artifact is
   for — a learner reads content; a repository holds code. Nothing here loosens CLAUDE.md §6: agents propose,
   humans approve.

4. **`11-ai-content-pipeline.md` describes a workflow with no user.** It is superseded by this ADR, not
   deleted: it is the design of record for the pipeline should content production ever return, and ADR-0010
   Decision 5 already anticipates AI coming back post-MVP as a paid runtime feature.

5. **"✦ Claude ile taslak üret" is not built.** The control is drawn in `whystack-studio.html` and has no
   implementation. Under this ADR it acquires no reason to get one. The mockup is amended.

6. **`AiDraft` is renamed `Draft`.** The status held a model's output; it now holds a person's. A status whose
   name asserts something untrue about every row in it is a status that will mislead whoever reads the table
   next. This is a rename across the enum, the database, the client and the studio's labels, and it is done in
   its own change — it is a consequence of this decision, not a prerequisite for it.

7. **An author publishes their own topic in one transition: `Draft → Published`.** The intermediate stages
   modelled a team — an author, a technical reviewer, an editor, an approver — applying four different lenses.
   With one person and no model output to catch, four transitions are one person clicking four times to
   produce four audit rows describing one act.

   `TechnicalReview`, `EditorialReview` and `Approved` remain valid statuses and remain reachable. The ladder
   is not deleted; climbing it alone stops being mandatory. The day a second person writes a topic, it is
   there.

8. **The act is recorded.** The transition writes one `TopicReview` row. `TopicReviews` stays the trace of a
   human decision — which is why its foreign key is `NO_ACTION` while every other one cascades, and why
   deleting 1,432 accounts left it untouched. One honest row, not four fictional ones.

9. **The mandatory-beats gate is enforced by the transition, and this is not negotiable.** A topic may not
   reach a reader without ADR-0024's four beats: Hook, Checkpoint, Summary, Next.

   This has nothing to do with review. It is completeness, and it applies to human writing exactly as it
   applied to a model's. Completion is a correct Checkpoint answer, so a topic with no Checkpoint is a topic
   nobody can ever finish: the basamak never fills, no station ever goes gold, and no screen anywhere explains
   why.

10. **CLAUDE.md is amended, not worked around.** §4 loses "transitioning `AiDraft → Published`" and gains
    "publishing a topic that has not passed the mandatory-beats gate" and "publishing on a human's behalf — an
    agent may never transition a topic to Published". §1.5 stays exactly as written; it simply no longer has
    anything to govern here, and will again the day a model writes a topic.

## The defect this ADR walks past

The publish gate is impassable today, and that is a plain bug rather than a decision.

`TransitionTopicHandler` validates a `TopicDraft`, and `TopicDraft` carries `Sections` only. ADR-0024 replaced
sections with blocks; `TopicSections` has zero rows. So a finished block topic — all four beats present — is
refused with twelve errors naming section types the model retired and the studio has no boxes for. Meanwhile
`BlockSkeletons.MissingMandatory`, which has always known the real rule, is called only from `/validate`: the
"Doğrula" button, which is advisory and skippable. That is how this project already published two topics with
no Checkpoint at all.

**This is fixed whether or not this ADR is accepted.** It is ADR-0024 being implemented where it was missed.

## What this costs — read this before accepting

**The one-pass problem.** Writing and reviewing are different mental states, and the ladder's real value to a
solo author was never the four clicks — it was the gap between them. You write on Tuesday and review on
Thursday, and on Thursday you see what Tuesday could not. Publishing in one act makes it frictionless to write
and ship in one breath, and the errors that survive that are exactly the ones a second look catches. This ADR
does not stop you leaving a topic overnight; it stops the system making you.

**The gate becomes load-bearing.** With three stages gone, the beats check is the only thing standing between a
draft and a reader. That is why Decision 9 says the transition must enforce it and the advisory button must
not be the enforcement.

**One trail, less granular.** "Technically reviewed by X, editorially by Y" becomes "published by Tolga". For
one person the first was fiction. For two it is not — revisit this the day someone else writes a topic.

## Alternatives rejected

**Keep the ladder.** Its purpose was catching model output. There is none. Ceremony that costs a step per topic
forever, on the thing the product most needs more of.

**Publish on save.** A save is a keystroke. Publishing is a decision, and a decision should be an act somebody
took on purpose — if only so that "I did not mean to publish that" stays a sentence nobody has to say.

**Auto-advance the stages on one click.** Four audit rows from one click is a trail that lies: it records
reviews that did not happen. Worse than no trail, because it looks like one.

**Leave ADR-0010 alone and change only the publishing flow.** Rejected: it would leave "MVP AI scope = content
production only" written down and false, and a future reader would follow it. A document that contradicts the
project is a defect, not a historical note.

## Consequences

- ADR-0010's Decision 1 is superseded; its Decisions 2-5 stand, including the post-MVP Premium assistant.
- `AiGeneratedDrafts` has no writer. `AiProviders` / `AiUsageEvents` stay in the schema, still unexercised.
- The provider abstraction stays (ADR-0010 "What This Preserves") — the assistant may return post-MVP.
- `TransitionTopicHandler` validates blocks; `ContentLifecycle.MayTransition` permits `Draft → Published`.
- The studio's publish panel becomes one action that states what publishing asserts.
- `PublishGateTests` inverts: it documents the defect today and will document the rule.
- The dead section model (30 files, 0 rows) loses its last caller and can go in its own change.
- `AiDraft → Draft` rename: enum, migration, api-client, studio labels. Its own change.
