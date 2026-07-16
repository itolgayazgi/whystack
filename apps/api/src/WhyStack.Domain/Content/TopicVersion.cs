namespace WhyStack.Domain.Content;

/// <summary>One revision of a topic's content (`07` — Topic Versioning Domain).</summary>
/// <remarks>
/// There is no <c>MarkdownPath</c> any more. ADR-0020 moved the source of truth into the database: the text
/// is in <see cref="TopicSection"/> and <see cref="ImplementationSection"/>, and <c>content/</c> is written
/// back OUT of here as the published record.
/// </remarks>
public class TopicVersion
{
    public Guid Id { get; init; }

    public required Guid TopicId { get; init; }

    /// <summary>1 for the first draft. A meaningful revision of published content creates the next one.</summary>
    public required int VersionNumber { get; init; }

    public required ContentStatus Status { get; set; }

    /// <summary>English. A translation with no canonical source cannot be reviewed against anything.</summary>
    public required string CanonicalLanguageCode { get; set; }

    public required int EstimatedReadingMinutes { get; set; }

    /// <summary>Content decays. A topic nobody has looked at in three years should be able to say so.</summary>
    public required DateOnly LastReviewedOn { get; set; }

    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? DeprecatedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Two editors, one topic, two browser tabs.
    /// </summary>
    /// <remarks>
    /// Now that content is authored in the application rather than merged in git, the concurrent-edit
    /// problem is ours. Without this, the second save silently discards the first — and nobody is told,
    /// because both requests succeeded.
    /// </remarks>
    public byte[]? RowVersion { get; set; }

    public Topic? Topic { get; init; }
    public ICollection<TopicTranslation> Translations { get; init; } = [];

    /// <summary>The topic's body as an ordered block flow (ADR-0024). Replaces sections and implementations.</summary>
    public ICollection<TopicBlock> Blocks { get; init; } = [];

    // Retired by ADR-0024 (blocks replace sections), kept until the content migration lands so the schema and
    // the existing rows still map. Removed once every topic is blocks.
    public ICollection<TopicSection> Sections { get; init; } = [];
    public ICollection<TopicImplementation> Implementations { get; init; } = [];
    public ICollection<TopicSupportedVersion> SupportedVersions { get; init; } = [];
}

/// <summary>
/// The editorial lifecycle (`10` § Topic Lifecycle). ORDERED: a topic advances one stage at a time.
/// </summary>
/// <remarks>
/// <c>AiDraft → Published</c> is forbidden by name in CLAUDE.md §1.5, and it is unreachable here rather
/// than merely discouraged — see <see cref="ContentLifecycle.MayTransition"/>. Moving the source of truth
/// into the database (ADR-0020) moved the gate; it did not open it.
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
    /// Backwards is not a violation — a reviewer who finds a problem sends it as far back as it needs to go,
    /// and that is the review working. Forwards is one stage at a time, because every gate between a draft
    /// and a reader is a gate a human has to open.
    /// </remarks>
    public static bool MayTransition(ContentStatus from, ContentStatus to) =>
        to == from + 1 || to < from;

    /// <summary>`04`: draft content is not publicly accessible. Approved is not Published.</summary>
    public static bool IsPubliclyVisible(ContentStatus status) =>
        status is ContentStatus.Published or ContentStatus.Deprecated;
}

/// <summary>A technology version the topic's claims hold for.</summary>
public class TopicSupportedVersion
{
    public Guid Id { get; init; }
    public required Guid TopicVersionId { get; init; }
    public required string Version { get; init; }
}

/// <summary>
/// Who moved this topic, when, and why (`07` § Editorial Workflow Domain).
/// </summary>
/// <remarks>
/// This table is the price of ADR-0020. Content used to be reviewed in a pull request, and git answered
/// "who changed this, and why". It is no longer, so the database has to answer instead — and an editorial
/// gate with no record of who opened it is not a gate, it is a formality.
/// </remarks>
public class TopicReview
{
    public Guid Id { get; init; }

    public required Guid TopicVersionId { get; init; }

    public required Guid ReviewerId { get; init; }

    public required ContentStatus FromStatus { get; init; }
    public required ContentStatus ToStatus { get; init; }

    /// <summary>Optional, and it should not be. A rejection with no reason is a rejection nobody can act on.</summary>
    public string? Note { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}
