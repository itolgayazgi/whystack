namespace WhyStack.Domain.Content;

/// <summary>
/// One revision of a topic's educational content (`07` — Topic Versioning Domain).
/// </summary>
/// <remarks>
/// The Markdown is not here. <see cref="MarkdownPath"/> points into <c>content/</c> and
/// <see cref="ContentHash"/> says which bytes were imported — which is how a stale row is detected
/// without reading the file, and how a cache can be keyed by content rather than by time.
/// </remarks>
public class TopicVersion
{
    public Guid Id { get; init; }

    public required Guid TopicId { get; init; }

    /// <summary>1 for the first import. A meaningful revision of published content creates the next one.</summary>
    public required int VersionNumber { get; init; }

    public required ContentStatus Status { get; set; }

    /// <summary>English. A translation with no canonical source cannot be reviewed against anything.</summary>
    public required string CanonicalLanguageCode { get; set; }

    /// <summary>Repository-relative, so a row can always be traced to the file that produced it.</summary>
    public required string MarkdownPath { get; set; }

    /// <summary>SHA-256 of the canonical file. Detects a changed file without opening it.</summary>
    public required string ContentHash { get; set; }

    public required int EstimatedReadingMinutes { get; set; }

    /// <summary>Content decays. A topic nobody has looked at in three years should be able to say so.</summary>
    public required DateOnly LastReviewedOn { get; set; }

    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? DeprecatedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; set; }

    public byte[]? RowVersion { get; set; }

    public Topic? Topic { get; init; }
    public ICollection<TopicTranslation> Translations { get; init; } = [];
    public ICollection<TopicSection> Sections { get; init; } = [];
    public ICollection<TopicSupportedVersion> SupportedVersions { get; init; } = [];
}

/// <summary>
/// The editorial lifecycle (`10` § Topic Lifecycle). ORDERED: a topic advances one stage at a time.
/// </summary>
/// <remarks>
/// <c>AiDraft → Published</c> is forbidden by name in CLAUDE.md §1.5, and it is unreachable here rather
/// than merely discouraged — see <see cref="ContentLifecycle.MayTransition"/>.
/// </remarks>
public enum ContentStatus
{
    Idea = 1,
    Outline = 2,
    AiDraft = 3,
    TechnicalReview = 4,
    EditorialReview = 5,
    Approved = 6,
    Published = 7,
    Deprecated = 8,
    Archived = 9,
}

public static class ContentLifecycle
{
    /// <summary>Forward by exactly one stage, or back to any earlier one.</summary>
    /// <remarks>
    /// Backwards is not a violation — a reviewer who finds a problem sends it as far back as it needs to
    /// go, and that is the review working. Forwards is one stage at a time, because every gate between a
    /// draft and a reader is a gate a human has to open.
    /// </remarks>
    public static bool MayTransition(ContentStatus from, ContentStatus to) =>
        to == from + 1 || to < from;

    /// <summary>`04`: draft content is not publicly accessible. Approved is not Published.</summary>
    public static bool IsPubliclyVisible(ContentStatus status) =>
        status is ContentStatus.Published or ContentStatus.Deprecated;
}

/// <summary>
/// A technology version this topic's claims hold for ("8", "9", "10").
/// </summary>
/// <remarks>
/// A child table rather than a comma-separated column, because this is the thing every "does this apply
/// to my .NET?" query filters on — and a query that has to <c>LIKE '%9%'</c> a string column cannot use
/// an index and will match "19".
/// </remarks>
public class TopicSupportedVersion
{
    public Guid Id { get; init; }
    public required Guid TopicVersionId { get; init; }
    public required string Version { get; init; }
}
