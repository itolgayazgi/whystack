using System.Text.Json;

namespace WhyStack.Application.Content;

/// <summary>
/// Which language the reader ASKED for, and which one they GOT (`08` § Response Metadata).
/// </summary>
/// <remarks>
/// CLAUDE.md §1.7 and `08` Principle 07 both forbid a silent fallback. When a Turkish reader opens a topic
/// that has not been translated, they are shown the English text — and they are TOLD.
///
/// The alternative is an app that quietly serves English to somebody who set their content language to
/// Turkish. They would conclude the translation is bad, or that the setting does not work. Neither is true,
/// and neither is something they can report, because nothing on the screen says what happened.
/// </remarks>
public sealed record LanguageResolution(
    string Requested,
    string Returned,
    bool FallbackUsed,
    string? FallbackReason)
{
    public static LanguageResolution Exact(string language) => new(language, language, false, null);

    public static LanguageResolution FellBackTo(string requested, string returned) =>
        new(requested, returned, true, "translation_not_available");
}

/// <summary>A topic in a list. Enough to decide whether to open it; not enough to read it.</summary>
public sealed record TopicSummary(
    Guid Id,
    string StableKey,
    string Slug,
    string Title,
    string? Summary,
    string LineKey,
    string LineName,

    /// <summary>The theme this topic threads through, or null (ADR-0023). What the roadmap slice groups on.</summary>
    string? ScopeKey,
    string? ScopeName,

    string Category,
    string Level,
    IReadOnlyList<string> SupportedVersions,
    int EstimatedReadingMinutes,
    string Status,
    LanguageResolution Language);

/// <summary>A topic, in full, in one language.</summary>
public sealed record TopicDetail(
    Guid Id,
    string StableKey,
    string Slug,
    string Title,

    /// <summary>
    /// The area the line belongs to. Asked through the line, never stored twice.
    /// </summary>
    /// <remarks>
    /// Here because a stop must be able to name the way back to its line, and the line map is addressed by
    /// area AND line. Without it the reader would have to guess — and a guess defaults to "backend", which
    /// sends every frontend reader to the wrong map.
    /// </remarks>
    string AreaKey,

    string LineKey,
    string LineName,
    string Category,

    /// <summary>"Konu tipi" in the design's künye: the explanation's shape (ADR-0024).</summary>
    string Archetype,

    string Level,
    IReadOnlyList<string> SupportedVersions,
    int EstimatedReadingMinutes,
    string Status,
    DateOnly LastReviewedOn,
    LanguageResolution Language,

    /// <summary>The concept. Written once, and true in every ecosystem (ADR-0021).</summary>
    IReadOnlyList<TopicSectionContent> Sections,

    /// <summary>The `[ .NET ▾ ]` panel. Empty for a topic with no code — "what is a transaction?".</summary>
    IReadOnlyList<TopicImplementationView> Implementations,

    TopicGraph Graph,

    /// <summary>Where this stop stands on its line, and the stops either side of it. Null if it is not on one.</summary>
    LineStop? Stop,

    /// <summary>
    /// The block flow the reader renders (ADR-0024): the shared blocks plus the chosen ecosystem's, in order.
    /// </summary>
    /// <remarks>
    /// Already merged and filtered — the client renders what it is given rather than re-deriving the rule.
    /// Sections and Implementations above are the retired model and go once every topic is blocks.
    /// </remarks>
    IReadOnlyList<TopicBlockView> Blocks);

/// <summary>
/// One block, ready to render. <c>Data</c> is the parsed object, not a string the client must parse again.
/// </summary>
public sealed record TopicBlockView(int Order, string Type, string? EcosystemKey, JsonElement Data);

/// <summary>One section: what it is, and the Markdown that fills it.</summary>
public sealed record TopicSectionContent(string SectionType, string Markdown);

/// <summary>How one ecosystem does it. <c>IsPreferred</c> says which panel opens first, not which one exists.</summary>
public sealed record TopicImplementationView(
    string EcosystemKey,
    string EcosystemName,
    string SupportedVersions,
    bool IsPreferred,
    IReadOnlyList<TopicSectionContent> Sections);

/// <summary>
/// The graph-derived sections, rendered (ADR-0002, Decision 5).
/// </summary>
/// <remarks>
/// Prerequisites, related topics and the next topic are NOT sections of the Markdown — they are edges. The
/// client renders them as sections; the database stores them once, as relationships. This is the one shape
/// in which they are allowed to reach a screen.
/// </remarks>
public sealed record TopicGraph(
    IReadOnlyList<TopicLink> Prerequisites,
    IReadOnlyList<TopicLink> Related,
    IReadOnlyList<TopicLink> Next);

public sealed record TopicLink(string StableKey, string Slug, string Title);

/// <summary>
/// A stop's place on its line: "DURAK 4/12", and the stops either side of it.
/// </summary>
/// <remarks>
/// <b>This is position, not history.</b> <c>Previous</c> is the stop BEFORE this one on the route — not the
/// page the reader came from. A visitor who arrived from a search engine never saw it, so the UI must name
/// it ("← Önceki durak: …") rather than call it "geri". A back affordance that lands somewhere the reader
/// has never been is a back affordance that lied.
///
/// <c>Previous</c> is null on the first stop and <c>Next</c> on the last; both are null on a line of one.
/// Those are facts about the route, not gaps — the reader says something different for each, and a caller
/// that treats null as "not loaded yet" would render a spinner forever on stop one.
///
/// Ordering and the published filter both come from the infrastructure's LineOrder, which the line map reads
/// too. That is deliberate: <c>Position</c> is a promise about a route the map draws, and two screens
/// disagreeing about a route is the kind of bug nobody reports because neither screen looks broken.
/// </remarks>
public sealed record LineStop(int Position, int Total, TopicLink? Previous, TopicLink? Next);

/// <summary>`08` § Pagination. A collection endpoint without a page size is an outage waiting for a corpus.</summary>
public sealed record Page<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}
