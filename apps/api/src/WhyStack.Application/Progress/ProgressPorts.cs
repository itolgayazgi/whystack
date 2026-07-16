namespace WhyStack.Application.Progress;

/// <summary>Reading progress and the streak (ADR-0025). EF Core stays on the far side of this line.</summary>
public interface IProgressRepository
{
    /// <summary>
    /// Record where a reader is. Returns null when the topic or ecosystem does not exist.
    /// </summary>
    /// <remarks>
    /// The block count comes back so the caller can clamp — see <see cref="RecordProgressHandler"/>. The
    /// client's number is a number anybody can send.
    /// </remarks>
    Task<RecordOutcome?> RecordAsync(
        Guid userId,
        string slug,
        string? ecosystemKey,
        int lastBlockOrder,
        bool? completed,
        DateOnly today,
        CancellationToken cancellationToken);

    /// <summary>Everything the home screen shows, in one query set rather than one per widget.</summary>
    Task<HomeSnapshot> HomeAsync(
        Guid userId,
        string? ecosystemKey,
        string language,
        CancellationToken cancellationToken);
}

public sealed record RecordOutcome(int LastBlockOrder, bool IsCompleted, int TotalBlocks);

/// <summary>
/// The home screen's state (ADR-0025). One shape, one round trip.
/// </summary>
/// <remarks>
/// The design shows a continue card, a basamak chart, a streak and a next-up list. Fetching those separately
/// would be four requests on the screen a reader opens first, every time.
/// </remarks>
public sealed record HomeSnapshot(
    StreakView Streak,

    /// <summary>Null for a reader who has not started anything. The home says so rather than faking a card.</summary>
    ContinueView? Continue,

    IReadOnlyList<LevelProgressView> Levels,
    IReadOnlyList<NextTopicView> Next);

public sealed record StreakView(int Current, int Longest);

public sealed record ContinueView(
    string Slug,
    string Title,
    string? EcosystemKey,
    int LastBlockOrder,
    int TotalBlocks,
    int EstimatedReadingMinutes);

/// <summary>
/// One rung of the basamak chart.
/// </summary>
/// <remarks>
/// <c>Total</c> is the corpus AS IT WAS when the reader arrived at this level, not as it stands now — so
/// publishing never lowers anybody's percentage. <c>Fresh</c> is what has opened since: the design's
/// "10/11 · 1 yeni". A reward, never a debt.
/// </remarks>
public sealed record LevelProgressView(string Level, int Completed, int Total, int Fresh);

public sealed record NextTopicView(
    string Slug,
    string Title,
    string Level,
    string DomainName,
    int EstimatedReadingMinutes);
