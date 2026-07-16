using WhyStack.Domain.Content;

namespace WhyStack.Application.Content;

/// <summary>Reads and writes topics. EF Core stays on the far side of this line (CLAUDE.md §3).</summary>
public interface ITopicRepository
{
    Task<Page<TopicRecord>> ListAsync(TopicQuery query, CancellationToken cancellationToken);

    Task<TopicRecord?> FindBySlugAsync(string slug, bool includeDrafts, CancellationToken cancellationToken);

    /// <summary>Titles for a set of topic ids — what turns a graph edge into a link a reader can read.</summary>
    Task<IReadOnlyDictionary<Guid, TopicLink>> LinksForAsync(
        IReadOnlyCollection<Guid> topicIds,
        string language,
        CancellationToken cancellationToken);

    /// <summary>
    /// The published stops of a line, in the line's own order — ids only.
    /// </summary>
    /// <remarks>
    /// Ids rather than links: the reader needs the whole sequence to know WHERE it stands, but it only ever
    /// renders the two stops beside it. Projecting titles for all of them would fetch a line's worth of
    /// translations to throw them away, on the page a reader waits longest for.
    ///
    /// Bounded by the line, and that bound is real but not enforced: a line is a curated route (the design
    /// draws twelve). The line map already loads this same set in full to draw itself, so if a line ever grows
    /// past a few hundred stops, both this and the map need paging — not just this.
    /// </remarks>
    Task<IReadOnlyList<Guid>> StopsOnLineAsync(string lineKey, CancellationToken cancellationToken);

    /// <summary>Everything an editor needs to fill the "İçerik Üret" form: domains, ecosystems, sections.</summary>
    Task<AuthoringCatalog> CatalogAsync(CancellationToken cancellationToken);

    /// <summary>The approved terms, for the validator. Small, and loaded whole — nothing queries an alias.</summary>
    Task<IReadOnlyCollection<Validation.TerminologyEntry>> TerminologyAsync(CancellationToken cancellationToken);
}

/// <summary>
/// A topic, as it comes out of the database — <b>with its words</b>.
/// </summary>
/// <remarks>
/// Under ADR-0018 this carried a path and a hash, and the API opened a file. ADR-0020 moved the source of
/// truth into the database, so the Markdown travels with the row and there is nothing to go and fetch.
/// </remarks>
public sealed record TopicRecord(
    Guid Id,
    string StableKey,
    string Slug,
    /// <summary>The AREA the line belongs to: "backend". Asked through the line, never stored twice.</summary>
    string AreaKey,
    string AreaName,

    /// <summary>The LINE this stop sits on: "b3-data-access" / "Veri Erişimi" (ADR-0027).</summary>
    string LineKey,
    string LineName,
    string? ScopeKey,
    string? ScopeName,
    string Category,

    /// <summary>The shape of the explanation — Mechanism, Comparison… (ADR-0024).</summary>
    string Archetype,

    string Level,
    string Status,
    string DefaultTitle,
    string CanonicalLanguage,
    int EstimatedReadingMinutes,
    DateOnly LastReviewedOn,
    IReadOnlyList<string> SupportedVersions,
    IReadOnlyList<TopicSectionRecord> Sections,
    IReadOnlyList<TopicImplementationRecord> Implementations,
    IReadOnlyList<TopicTranslationRecord> Translations,
    IReadOnlyList<TopicEdge> Edges,

    /// <summary>The block flow (ADR-0024). Sections above are retired and go once every topic is blocks.</summary>
    IReadOnlyList<TopicBlockRecord> Blocks);

/// <summary>One stored block, straight off the row. <c>DataJson</c> is parsed at the edge, not here.</summary>
public sealed record TopicBlockRecord(
    int Order,
    string Type,
    string LanguageCode,

    /// <summary>Null is a SHARED block — the why, written once. A key marks one ecosystem's treatment.</summary>
    string? EcosystemKey,

    string DataJson);

public sealed record TopicSectionRecord(string SectionTypeKey, string LanguageCode, string Markdown, int SortOrder);

/// <summary>What sits behind the `[ .NET ▾ ]` control (ADR-0021).</summary>
public sealed record TopicImplementationRecord(
    Guid Id,
    string EcosystemKey,
    string EcosystemName,
    string? LanguageKey,
    string? FenceLanguage,
    string SupportedVersions,
    IReadOnlyList<TopicSectionRecord> Sections);

public sealed record TopicTranslationRecord(string Language, string Title, string? Summary, string Status);

public sealed record TopicEdge(string Type, Guid ToTopicId);

/// <summary>
/// A page of topics. `08` Principle 09: a collection endpoint without a limit is an outage waiting for a
/// corpus to grow — and CLAUDE.md §4 forbids the unbounded query outright.
/// </summary>
public sealed record TopicQuery(
    /// <summary>The LINE key: "b3-data-access" (ADR-0027). Was Domain, which meant two things at once.</summary>
    string? Line,
    string? Level,
    int PageNumber,
    int PageSize,
    bool IncludeDrafts,

    /// <summary>Free text over titles and summaries. Null or blank means "no search", not "match nothing".</summary>
    string? Search = null);

/// <summary>The lists the authoring form is built from. All of them are reference data; none is user input.</summary>
public sealed record AuthoringCatalog(
    /// <summary>The "Hat" dropdown: B1..B8. NOT the areas — a topic is authored onto a line (ADR-0027).</summary>
    IReadOnlyList<LineOption> Lines,
    IReadOnlyList<ScopeOption> Scopes,

    /// <summary>
    /// The category names, straight from the <c>TopicCategory</c> enum.
    /// </summary>
    /// <remarks>
    /// A CLOSED classification (unlike Scope), so the studio must offer these as a dropdown — a free
    /// textbox lets an editor type "Perfromance" and turns a typo into a failed save. The list comes from the
    /// enum rather than a hardcoded copy in the client, so adding a member cannot leave the two out of step.
    /// </remarks>
    IReadOnlyList<string> Categories,

    /// <summary>The archetypes and the block skeleton each one scaffolds (ADR-0024).</summary>
    IReadOnlyList<ArchetypeOption> Archetypes,

    /// <summary>Every block type the editor may add, and whether it is one of the four required beats.</summary>
    IReadOnlyList<BlockTypeOption> BlockTypes,

    IReadOnlyList<EcosystemOption> Ecosystems,
    IReadOnlyList<SectionTypeOption> SectionTypes,
    IReadOnlyList<TopicOption> Topics);

/// <summary>
/// An archetype and the flow it starts from (ADR-0024).
/// </summary>
/// <remarks>
/// The skeleton is a SUGGESTION the editor reshapes — sending it from the server rather than hardcoding it in
/// the studio keeps one definition of "what a Mechanism looks like", which is the whole reason the section
/// template was abandoned for something the author can bend.
/// </remarks>
public sealed record ArchetypeOption(string Key, IReadOnlyList<string> Skeleton);

public sealed record BlockTypeOption(string Key, bool IsMandatory);

/// <summary>
/// One line for the studio's "Hat" dropdown: B1 Dil &amp; Runtime, B3 Veri Erişimi (ADR-0027).
/// </summary>
/// <remarks>
/// <c>AreaName</c> so the dropdown can group. Eight lines is a long flat list, and an author picking one
/// thinks "Backend, data access" — not "b3".
/// </remarks>
public sealed record LineOption(string Key, string Name, string AreaKey, string AreaName);

/// <summary>A theme a topic may be tagged with (ADR-0023). Curated in the studio.</summary>
/// <summary>
/// One scope for the studio's "Kapsam" dropdown: EF Core, Async / Await (ADR-0027).
/// </summary>
/// <remarks>
/// <c>LineKey</c> is what makes the dropdown filterable. A scope only means something on its line — B1's
/// "Eşzamanlılık" and B3's "Transaction &amp; Eşzamanlılık" are two neighbourhoods — so offering every
/// scope on every line would invite exactly the mix-up the composite key exists to prevent.
/// </remarks>
public sealed record ScopeOption(string Key, string Name, string LineKey);

public sealed record EcosystemOption(
    string Key,
    string Name,
    bool IsAvailable,
    IReadOnlyList<LanguageOption> Languages);

public sealed record LanguageOption(string Key, string Name, string FenceLanguage);

/// <summary>A section the editor may write, and where it belongs (ADR-0021).</summary>
public sealed record SectionTypeOption(
    string Key,
    int SortOrder,
    SectionScope Scope,
    bool IsMandatory,
    bool IsGraphDerived);

/// <summary>An existing topic, so a new one can be related to it without the editor typing a stable key.</summary>
public sealed record TopicOption(Guid Id, string StableKey, string Title);
