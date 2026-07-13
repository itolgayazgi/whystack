namespace WhyStack.Application.Content;

/// <summary>Reads topics. EF Core stays on the far side of this line (CLAUDE.md §3).</summary>
public interface ITopicRepository
{
    Task<Page<TopicRecord>> ListAsync(TopicQuery query, CancellationToken cancellationToken);

    Task<TopicRecord?> FindBySlugAsync(string slug, bool includeDrafts, CancellationToken cancellationToken);

    /// <summary>Titles for a set of topic ids — what turns a graph edge into a link a reader can read.</summary>
    Task<IReadOnlyDictionary<Guid, TopicLink>> LinksForAsync(
        IReadOnlyCollection<Guid> topicIds,
        string language,
        CancellationToken cancellationToken);
}

/// <summary>
/// What the repository returns: the row, plus the paths to the words.
/// </summary>
/// <remarks>
/// The Markdown is NOT here, and that is `07`'s design, not an omission: "Markdown may exist in files.
/// The database stores metadata, relationships and publishing state." What travels out of the database is
/// a path and a hash; the content reader turns them into text (ADR-0018).
/// </remarks>
public sealed record TopicRecord(
    Guid Id,
    string StableKey,
    string Slug,
    string Technology,
    string Category,
    string Level,
    string Status,
    string DefaultTitle,
    string CanonicalLanguage,
    string CanonicalMarkdownPath,
    string CanonicalContentHash,
    int EstimatedReadingMinutes,
    DateOnly LastReviewedOn,
    IReadOnlyList<string> SupportedVersions,
    IReadOnlyList<string> SectionTypes,
    IReadOnlyList<TopicTranslationRecord> Translations,
    IReadOnlyList<TopicEdge> Edges);

public sealed record TopicTranslationRecord(string Language, string Title, string MarkdownPath, string ContentHash);

public sealed record TopicEdge(string Type, Guid ToTopicId);

/// <summary>
/// A page of topics. `08` Principle 09: a collection endpoint without a limit is an outage waiting for a
/// corpus to grow — and CLAUDE.md §4 forbids the unbounded query outright.
/// </summary>
public sealed record TopicQuery(
    string? Technology,
    string? Level,
    int PageNumber,
    int PageSize,
    bool IncludeDrafts);

/// <summary>
/// Turns a Markdown path into Markdown.
/// </summary>
/// <remarks>
/// A port, not a file read inline in a handler — because the Application layer is not allowed to know
/// there is a file system (CLAUDE.md §3), and because an offline Knowledge Pack will one day supply the
/// same text from an archive rather than a directory.
///
/// Keyed by CONTENT HASH, not by path or by time. A cached page keyed by a timestamp is invalidated by a
/// deploy that changed nothing; a page keyed by its content is invalidated exactly when the content
/// changes, which is the only correct answer.
/// </remarks>
public interface ITopicContentReader
{
    Task<IReadOnlyList<TopicSectionContent>> ReadSectionsAsync(
        string markdownPath,
        string contentHash,
        IReadOnlyList<string> sectionTypes,
        CancellationToken cancellationToken);
}
