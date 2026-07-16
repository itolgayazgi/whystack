using WhyStack.Domain.Content;

namespace WhyStack.Domain.Users;

/// <summary>
/// Where a reader is in one topic, on one ecosystem's line (ADR-0025).
/// </summary>
/// <remarks>
/// Keyed by (reader, topic, ecosystem) because finishing "the .NET line" means finishing each station's .NET
/// treatment (ADR-0024): the shared "why" is the same on every line, the treatment is not. Someone who did
/// `async` in .NET has not thereby done it in Java.
///
/// It holds a POSITION, not a transcript. A row per block would be the maximalist model and would cost a
/// write on every scroll to answer the one question the product asks — "where was I?" — which position and
/// completion already answer.
/// </remarks>
public class UserTopicProgress
{
    public Guid Id { get; init; }

    public required Guid UserId { get; init; }

    public required Guid TopicId { get; init; }

    /// <summary>Null for a topic with no code — it has one treatment, and it is everyone's.</summary>
    public string? EcosystemKey { get; init; }

    /// <summary>The block the reader last reached. What "Devam et" returns them to.</summary>
    public required int LastBlockOrder { get; set; }

    /// <summary>
    /// Set when the READER says so, never inferred from reaching the last block.
    /// </summary>
    /// <remarks>
    /// Scrolling to the bottom is not understanding. Inferring completion from a scroll offset would let the
    /// product tell somebody they had learned something because their thumb moved (ADR-0025, Decision 3).
    /// </remarks>
    public bool IsCompleted { get; set; }

    public DateTime StartedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public Topic? Topic { get; init; }
}

/// <summary>
/// The streak (ADR-0025, Decision 4) — consecutive days, and the product's one gamification mechanic.
/// </summary>
/// <remarks>
/// <b>`09` forbids this by name</b> ("Not a gamified social application"), and ADR-0025 records the owner's
/// deliberate override. It is written down so it stays a decision with an author rather than something the
/// product drifted into.
///
/// Three columns, not a row per active day: a table of days would let us draw a contribution graph nobody
/// asked for and would grow forever. These answer the only question the design asks.
/// </remarks>
public class UserStreak
{
    /// <summary>The reader. One row each — the key IS the user.</summary>
    public required Guid UserId { get; init; }

    public int CurrentStreak { get; set; }

    public int LongestStreak { get; set; }

    /// <summary>
    /// A DATE, not an instant.
    /// </summary>
    /// <remarks>
    /// A streak is about days. Storing a timestamp invites comparing across time zones and telling a reader
    /// in Istanbul they broke a streak they did not — the classic way this feature turns hostile.
    /// </remarks>
    public required DateOnly LastActiveOn { get; set; }

    /// <summary>
    /// Move the streak to <paramref name="today"/>. Same day does nothing; yesterday extends; a gap resets.
    /// </summary>
    /// <remarks>
    /// A pure method on the entity rather than logic in a handler, because "what is a streak" is one rule and
    /// this is the only place that gets to answer it.
    /// </remarks>
    public void Touch(DateOnly today)
    {
        if (LastActiveOn == today) return;

        CurrentStreak = LastActiveOn.AddDays(1) == today ? CurrentStreak + 1 : 1;
        LongestStreak = Math.Max(LongestStreak, CurrentStreak);
        LastActiveOn = today;
    }
}
