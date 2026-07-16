namespace WhyStack.Application.Roadmap;

/// <summary>
/// The map: one LINE's stops, in order, with this reader's state on each (ADR-0027).
/// </summary>
/// <remarks>
/// A line is B1 Dil &amp; Runtime, B3 Veri Erişimi — a route through an area. The ECOSYSTEM is not a line:
/// it is which network you are looking at. Choosing Java does not add a route beside .NET; it rebuilds the
/// same eight routes in Java.
/// </remarks>
public interface IRoadmapRepository
{
    /// <summary>Null when the ecosystem or the line does not exist.</summary>
    Task<RoadmapView?> GetAsync(
        Guid userId,
        string ecosystemKey,
        string lineKey,
        string language,
        CancellationToken cancellationToken);

    /// <summary>The tabs: every ecosystem, including the ones with nothing on them yet.</summary>
    Task<IReadOnlyList<EcosystemOption>> EcosystemsAsync(CancellationToken cancellationToken);

    /// <summary>The sidebar's areas: Backend, Frontend, Database, DevOps.</summary>
    Task<IReadOnlyList<AreaOption>> AreasAsync(CancellationToken cancellationToken);

    /// <summary>The lines inside an area — B1..B8 for Backend. Empty when the area has none yet.</summary>
    Task<IReadOnlyList<LineOption>> LinesAsync(string areaKey, CancellationToken cancellationToken);
}

/// <summary>
/// One ecosystem tab — the network switcher, not a route through it (ADR-0027).
/// </summary>
/// <remarks>
/// The unavailable ones are RETURNED, not filtered out — the design shows them greyed with a "YAKINDA"
/// badge. A promise the reader can see is worth more than a shorter list, and hiding them would make the
/// product look like it thinks .NET is all there is.
/// </remarks>
public sealed record EcosystemOption(string Key, string Name, bool IsAvailable, int TopicCount);

/// <summary>
/// One entry in the sidebar's area rail.
/// </summary>
/// <remarks>
/// The count is here so the UI never has to guess. An area with nothing published still appears — hiding it
/// would make the rail's shape depend on the content pipeline, and a reader would watch "Frontend" blink
/// into existence one day for no reason they can see.
/// </remarks>
public sealed record AreaOption(string Key, string Name, int TopicCount);

/// <summary>
/// One line inside an area: the map's route, and its colour.
/// </summary>
/// <remarks>
/// The colour travels with the line because a line is a ROW — an editor can add one, and a colour that
/// lived in a TypeScript file would be a colour the editor cannot give it. The palette it is drawn from is
/// still the design system's; the seed is what enforces that (ADR-0027).
/// </remarks>
public sealed record LineOption(string Key, string Name, string Color, int TopicCount);

public sealed record RoadmapView(
    string EcosystemKey,
    string EcosystemName,
    string LineKey,
    string LineName,
    string LineColor,
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

    /// <summary>The neighbourhood this stop stands in: "EF Core". Null for a standalone stop (ADR-0027).</summary>
    string? ScopeKey,
    string? ScopeName,

    int EstimatedReadingMinutes,
    StationState State,

    /// <summary>0–100, from the reader's furthest block. 0 for a station they never opened.</summary>
    int Percent,

    /// <summary>Its place in a numbered chain — "II / III" — or null (ADR-0027).</summary>
    SequenceView? Sequence,

    /// <summary>
    /// Where this station meets another AREA's line — the design's "⇄ Aktarma".
    /// </summary>
    /// <remarks>
    /// A Related edge that leaves the area, which is the only kind worth drawing. Under ADR-0027 the
    /// boundary moved: it used to be the domain, but domains conflated areas and lines, so "crosses a
    /// domain" quietly meant "crosses a line" for half the rows — and a link from B3 to B1 is not a transfer,
    /// it is the next stop on the same network. A symbol that marks everything marks nothing.
    /// </remarks>
    TransferView? Transfer);

public sealed record TransferView(string Slug, string Title, string AreaName);

/// <summary>A stop's place in a numbered chain: "II / III" (ADR-0027).</summary>
public sealed record SequenceView(string Group, int Part, int Of);
