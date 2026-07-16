# ADR-0026 — SkillLevel is stored as a spaced integer

- **Status:** Accepted
- **Date:** 2026-07-16
- **Deciders:** Tolga Yazgı (owner)
- **Supersedes:** nothing — this decision had no home before
- **Related:** `07-database-design.md`, `08-api-standards.md` (Enum Standard), ADR-0002

---

## Context

`SkillLevel` is the spine of the product. The basamak — Junior → MidLevel → Senior → Expert — is what
the roadmap, the line map and the home screen are all built on, and "which rung is higher" is not a
presentation detail. It is the meaning.

It was persisted with `HasConversion<string>()`, so `Topics.DefaultLevel` was `nvarchar(16)` holding
`'Junior'`, `'MidLevel'`. That produced a defect that shipped in **three** separate queries:

```sql
ORDER BY DefaultLevel      -- sorts the TEXT: Expert, Junior, MidLevel, Senior
```

Alphabetical, and almost exactly backwards. Nothing throws, the page renders, and the line looks like a
line — it simply starts at the wrong end. It was caught only by a test that seeded a Junior topic named
"Aaa" and an Expert named "Zzz", because sorting by title alone would have looked correct.

The fix in place was a `CASE` expression in one helper (`LevelOrdering`). That works, but it cannot use
`IX_Topics_DomainId_DefaultLevel` — the database must sort — and it leaves the ordering as a fact that
lives outside the data.

No document had decided how an enum is STORED. `08` governs the **wire** only ("Enums must be serialized
as strings"); `07` and the ADRs are silent on the column.

## Decision

**Store `SkillLevel` as an integer, with values spaced by ten:**

```csharp
public enum SkillLevel
{
    Junior = 10,
    MidLevel = 20,
    Senior = 30,
    Expert = 40,
}
```

Applies to every persisted `SkillLevel`: `Topics.DefaultLevel` and `UserPreferences.PreferredSkillLevel`.

**Each column carries a CHECK constraint naming the permitted values.**

**The wire does not change.** `08`'s Enum Standard is untouched: `JsonStringEnumConverter` serialises by
NAME, so the API still answers `"level": "Junior"` and still accepts `?level=junior`. The number never
leaves the database. Numeric enum values on the wire remain forbidden (CLAUDE.md §4).

## Rationale

**The spacing is the point.** Plain 1–4 works only while the enum's numeric order equals the semantic
order. The first inserted rung breaks it: a `Staff` level between Senior and Expert forces either a
renumber — which rewrites the meaning of every row already written, and makes every existing backup and
export lie — or `Staff = 5` appended at the end, which puts `ORDER BY` back to being wrong and brings the
`CASE` back. That outcome is the worst of both: integer storage *and* a CASE. Spacing makes the insertion
a one-line change: `Staff = 35`.

**What this buys:** `ORDER BY DefaultLevel` is correct by construction and can use the existing indexes.
The ordering stops being a rule that three queries had to remember and start being a property of the data.

**What this costs, stated plainly:**

- The database is no longer readable by eye. `SELECT DefaultLevel FROM Topics` answers `20`, and every
  bug report, manual query and support conversation needs the decoder ring above. This is a real loss and
  the main argument that was weighed against it.
- The enum's numbers are now data. They are in every row, so they may never be renumbered — only
  inserted between. The spacing is what makes that possible, and it is why the numbers look arbitrary.

**The CHECK constraint is not decoration.** Earlier in this project `Archetype` was stored as an int by
accident — the `HasConversion<string>()` was missing — and the column held **`0`**, a value the enum does
not define. Nothing complained. String storage made that class of corruption loud for free; integer
storage does not, so the loudness is bought back explicitly:

```sql
CONSTRAINT CK_Topics_DefaultLevel CHECK (DefaultLevel IN (10, 20, 30, 40))
```

A row that does not mean anything now fails to be written, at the only layer that can promise it.

## Consequences

- `LevelOrdering`'s `CASE` is deleted. `OrderBy(topic => topic.DefaultLevel)` is correct again, and the
  helper's whole reason to exist is gone.
- Migration `SkillLevel_As_Int` alters both columns and backfills by name (`'Junior'` → 10, …). It is
  written so that a value it does not recognise fails the migration rather than landing as `0` — the
  silent-corruption path this ADR exists to close.
- Adding a rung means picking a number BETWEEN the neighbours, never appending. A reviewer who sees
  `Staff = 45` on a pull request should reject it: 45 is above Expert.
- **Adding a rung is two changes, not one:** the enum member, and a migration that widens both CHECK
  constraints to admit the new number. That is deliberate friction. The constraint is the thing standing
  between us and another silent `0`, and a rung that reaches production without it would be a rung the
  database has never agreed to.
- The wire, the URL and the OpenAPI document are unaffected. No client changes.
- If the corpus ever grows enough for the ordering to matter, the indexes now serve it. That was not the
  reason for this decision; it is a consequence of it.
