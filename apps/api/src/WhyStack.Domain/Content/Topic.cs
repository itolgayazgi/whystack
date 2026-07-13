using WhyStack.Domain.Users;

namespace WhyStack.Domain.Content;

/// <summary>
/// The canonical identity of an educational topic (`07` — Content Domain; `10`, ADR-0002).
/// </summary>
/// <remarks>
/// The database stores metadata, relationships and publishing state. It does NOT store the Markdown —
/// `07` is explicit: "Markdown may exist in files. The database stores metadata, relationships and
/// publishing state." <see cref="TopicVersion.MarkdownPath"/> and <see cref="TopicVersion.ContentHash"/>
/// are the link back to <c>content/</c>, which stays the single source of truth (ADR-0018).
/// </remarks>
public class Topic
{
    public Guid Id { get; init; }

    /// <summary>
    /// The identity, and it never changes. Every graph edge, quiz reference and roadmap entry resolves
    /// through this string — so renaming it is not a rename, it is a deletion and a different topic.
    /// </summary>
    public required string StableKey { get; init; }

    /// <summary>The URL segment. May be corrected; <see cref="StableKey"/> absorbs the consequences.</summary>
    public required string Slug { get; set; }

    /// <summary>
    /// The technology slug from <c>content/</c> ("csharp", "sql-server"). `07` models a Technology
    /// Catalog with a foreign key; that catalog is Sprint 5's, and this column becomes the key then.
    /// </summary>
    public required string Technology { get; set; }

    public required TopicCategory Category { get; set; }

    /// <summary>
    /// The same four levels a learner states about themselves (`07` — TopicLevels: "These levels are
    /// permanent product concepts"). One enum, not two: a topic written for a Mid-Level reader and a
    /// reader who calls themselves Mid-Level must mean the same thing, or the roadmap cannot match them.
    /// </summary>
    public required SkillLevel DefaultLevel { get; set; }

    /// <summary>The canonical (English) title, so a topic can be listed without loading a translation.</summary>
    public required string DefaultTitle { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<TopicVersion> Versions { get; init; } = [];
    public ICollection<TopicRelationship> OutgoingRelationships { get; init; } = [];
}

/// <summary>`10` § Content Categories. Extended by adding a member — an open list by design.</summary>
public enum TopicCategory
{
    Concept = 1,
    Syntax = 2,
    Architecture = 3,
    Performance = 4,
    Security = 5,
    Networking = 6,
    Database = 7,
    Cloud = 8,
    DevOps = 9,
    Testing = 10,
    DesignPattern = 11,
    FrameworkFeature = 12,
    LanguageFeature = 13,
    Tool = 14,
    Library = 15,
    Protocol = 16,
    Interview = 17,
    CaseStudy = 18,
}
