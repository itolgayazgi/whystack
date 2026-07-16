# ADR-0025 — Reading Progress, And A Streak That Overrides `09`

- Status: Accepted
- Date: 2026-07-16
- Owner: WhyStack Core Team
- Sources: the approved designs (basamak home, metro map, durak-içi); `09-ui-design-system.md` § Product Personality; `04-development-roadmap.md` Sprint 4 (Learning Status, Reading Progress) and Sprint 5 (Roadmap Engine); ADR-0024 (Block-Based Content)
- Overrides: `09`'s caution against gamification, for the streak specifically.

---

## Context

The approved home and map designs cannot be drawn from the corpus alone. They show *"%64"*, *"9 durak
kaldı"*, *"12 dk kaldı"*, a ✓ on finished stations, and **🔥 12 gün**. Every one of those is state about a
PERSON, and the product has none: there is no progress model. Until there is, the home screen is a layout with
nothing to say, and the reading screen's block map cannot mark anything done.

Two of those numbers are pedagogy and one is not, and the distinction is the whole reason this ADR exists.

## Decision

### 1. Progress is per (reader, topic, ecosystem)

Finishing "the .NET line" means completing each station's .NET treatment (ADR-0024): the shared "why" is the
same on every line, but the treatment is not. So progress is keyed by **(UserId, TopicId, EcosystemKey)**, and
a reader who did `async` in .NET has not thereby done it in Java.

`EcosystemKey` is nullable: a topic with no code has one treatment, and it is everyone's.

### 2. What is stored is the position, not a transcript

`UserTopicProgress` holds `LastBlockOrder`, `IsCompleted` and the timestamps. It does NOT hold a row per block.

A row per block would be the honest maximalist model, and it would be a table with (readers × topics × blocks)
rows to answer one question the product actually asks: *"where was I?"*. The block-level detail buys a
progress bar that is exact rather than proportional, and costs a write on every scroll. Position plus
completion answers every number the designs show.

### 3. Completion is a claim the reader makes, not one we infer from scrolling

Reaching the last block is not understanding it. `IsCompleted` is set when the reader says so — the design's
"Devam et" ends at a station, not at a scroll offset. Inferring it from scroll would let the product tell
somebody they have learned something because their thumb moved.

### 4. The streak is IN, and it overrides `09`

`09` says the interface must not feel like "a gamified social application" and lists gamification among the
things to avoid. A streak — consecutive days, reset to zero on a miss — is exactly that mechanic: it exists to
return you tomorrow, not to teach you today.

**The product owner chose it deliberately, having been shown that trade.** It ships. This ADR records the
override so it is a decision rather than something inherited from a mockup:

- **In:** the streak counter (🔥 N gün), progress bars, the basamak chart, checkpoints.
- **Out, still:** forced ordering. Topics are not locked behind prerequisites — the earlier decision that a
  reader picks their own way ("sıra dayatmıyoruz") stands, and nothing in the designs' lock icons overrides it
  until it is chosen as explicitly as the streak was.

`09` § Product Personality is amended: the interface avoids gamification *except* the streak, which is named.

### 5. The streak is three columns, not a table of days

`UserStreak(UserId, CurrentStreak, LongestStreak, LastActiveOn)`. On activity: same day → nothing; the day
before → +1; anything else → back to 1.

A row per active day would let us draw a contribution graph nobody asked for, and would grow forever. Three
columns answer the only question the design asks. `LastActiveOn` is a **date**, not a timestamp: a streak is
about days, and storing an instant invites comparing across time zones and telling a reader in Istanbul that
they broke a streak they did not.

### 6. Progress is written by the client, and the server does not trust the number

The client says "I am at block 7 of async/.NET". The server clamps that to the topic's real block count and
refuses an unknown topic or ecosystem — a number a client sends is a number anybody can send, and a progress
bar past 100% is the least of what a hand-written request would do.

## Consequences

- The home, the map and the block map can finally show what they were designed to show.
- The streak means the product is, by its own document's definition, partly gamified. That is now written down
  and attributable, not accidental.
- Progress rows are per reader per topic per ecosystem — bounded by the corpus, not by scrolling.
- Sprint 4 and Sprint 5 (`04`) are brought forward together, because the approved home needs both.

## Alternatives rejected

- **A row per block (`UserBlockProgress`, as the skeleton sketched).** Exact, and a write per scroll for a
  precision the designs never display. Position + completion is the same picture at a fraction of the cost.
- **Infer completion from reaching the last block.** Cheaper and dishonest: it would mark a topic learned
  because somebody scrolled to the bottom.
- **Keep the streak out.** Consistent with `09`, and against an explicit decision by the person whose product
  it is. The right response to that is to write the override down, not to quietly obey the document.
- **A daily activity table.** Grows forever, answers a question nothing asks.
