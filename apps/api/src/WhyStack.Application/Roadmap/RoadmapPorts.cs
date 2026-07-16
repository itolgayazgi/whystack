namespace WhyStack.Application.Roadmap;

/// <summary>The line map: one ecosystem's topics, in order, with this reader's state on each.</summary>
public interface IRoadmapRepository
{
    /// <summary>Null when the ecosystem or the domain does not exist.</summary>
    Task<RoadmapView?> GetAsync(
        Guid userId,
        string ecosystemKey,
        string domainKey,
        string language,
        CancellationToken cancellationToken);

    /// <summary>The tabs: every ecosystem, including the ones with nothing on them yet.</summary>
    Task<IReadOnlyList<LineOption>> LinesAsync(CancellationToken cancellationToken);
}

/// <summary>
/// One ecosystem tab.
/// </summary>
/// <remarks>
/// The unavailable ones are RETURNED, not filtered out — the design shows them greyed with a "YAKINDA"
/// badge. A promise the reader can see is worth more than a shorter list, and hiding them would make the
/// product look like it thinks .NET is all there is.
/// </remarks>
public sealed record LineOption(string Key, string Name, bool IsAvailable, int TopicCount);

public sealed record RoadmapView(
    string EcosystemKey,
    string EcosystemName,
    string DomainKey,
    string DomainName,
    IReadOnlyList<StationView> Stations);

/// <summary>Where the reader stands on one station.</summary>
/// <remarks>
/// <b>Ahead is not locked.</b> We do not impose an order — a reader arrives with their own question, and a
/// platform whose promise is "neden" cannot answer "not yet". The state is a suggestion the UI dims; every
/// station is reachable, and the API enforces no prerequisite because there is none to enforce.
/// </remarks>
public enum StationState
{
    /// <summary>The reader said they finished it. Their claim, not our inference.</summary>
    Done,

    /// <summary>Started, not finished. At most one — the design's "BURADASIN".</summary>
    Current,

    /// <summary>The nearest station they have not started. A suggestion, not a gate.</summary>
    Next,

    /// <summary>Further along the line. Dimmed, and still one click away.</summary>
    Ahead,
}

public sealed record StationView(
    string Slug,
    string Title,
    string Level,
    string? SubAreaName,
    int EstimatedReadingMinutes,
    StationState State,

    /// <summary>0–100, from the reader's furthest block. 0 for a station they never opened.</summary>
    int Percent,

    /// <summary>
    /// Where this station meets another domain's line — the design's "⇄ Aktarma".
    /// </summary>
    /// <remarks>
    /// A Related edge that crosses a DOMAIN boundary, which is the only kind worth drawing: a link inside
    /// Backend is just the next station, and marking it a transfer would make every station a transfer and
    /// the symbol mean nothing.
    /// </remarks>
    TransferView? Transfer);

public sealed record TransferView(string Slug, string Title, string DomainName);
