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
    string DomainKey,
    string DomainName,
    string? SubAreaKey,
    string? SubAreaName,
    string Category,
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
    string? Domain,
    string? Level,
    int PageNumber,
    int PageSize,
    bool IncludeDrafts);

/// <summary>The lists the authoring form is built from. All of them are reference data; none is user input.</summary>
public sealed record AuthoringCatalog(
    IReadOnlyList<DomainOption> Domains,
    IReadOnlyList<SubAreaOption> SubAreas,

    /// <summary>
    /// The category names, straight from the <c>TopicCategory</c> enum.
    /// </summary>
    /// <remarks>
    /// A CLOSED classification (unlike SubArea), so the studio must offer these as a dropdown — a free
    /// textbox lets an editor type "Perfromance" and turns a typo into a failed save. The list comes from the
    /// enum rather than a hardcoded copy in the client, so adding a member cannot leave the two out of step.
    /// </remarks>
    IReadOnlyList<string> Categories,

    IReadOnlyList<EcosystemOption> Ecosystems,
    IReadOnlyList<SectionTypeOption> SectionTypes,
    IReadOnlyList<TopicOption> Topics);

public sealed record DomainOption(string Key, string Name);

/// <summary>A theme a topic may be tagged with (ADR-0023). Curated in the studio.</summary>
public sealed record SubAreaOption(string Key, string Name);

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
