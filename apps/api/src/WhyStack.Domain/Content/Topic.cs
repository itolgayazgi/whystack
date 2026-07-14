using WhyStack.Domain.Users;

namespace WhyStack.Domain.Content;

/// <summary>
/// A CONCEPT (ADR-0021). It belongs to a domain — Backend, Database — not to a language.
/// </summary>
/// <remarks>
/// `Connection Pooling` is Backend. It is Backend in .NET and it is Backend in Java, and the reason it
/// exists is the same in both. The product's promise is *"why before how"*, and until this was modelled the
/// database could not tell the difference between the why (written once) and the how (written per
/// ecosystem).
///
/// A topic ABOUT a language — "What is C#?" — is still a topic. Its subject happens to be the language, and
/// its domain is Language. That is not the same thing as a concept implemented in one.
/// </remarks>
public class Topic
{
    public Guid Id { get; init; }

    /// <summary>
    /// The identity, and it never changes. Every graph edge, quiz reference and roadmap entry resolves
    /// through this string — so renaming it is not a rename: it is a deletion and a different topic.
    /// </summary>
    public required string StableKey { get; init; }

    /// <summary>The URL segment. May be corrected; <see cref="StableKey"/> absorbs the consequences.</summary>
    public required string Slug { get; set; }

    /// <summary>Backend, Database, Networking… The concept's home. NOT a language (ADR-0021).</summary>
    public required Guid DomainId { get; set; }

    public required TopicCategory Category { get; set; }

    /// <summary>
    /// The same four levels a learner states about themselves (`07` — TopicLevels: "These levels are
    /// permanent product concepts"). One enum, not two: a topic written for a Mid-Level reader and a reader
    /// who calls themselves Mid-Level must mean the same thing, or the roadmap cannot match them.
    /// </summary>
    public required SkillLevel DefaultLevel { get; set; }

    /// <summary>The canonical (English) title, so a topic can be listed without loading a translation.</summary>
    public required string DefaultTitle { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; set; }

    public KnowledgeDomain? Domain { get; init; }
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
