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

    /// <summary>
    /// The LINE this stop sits on: B1 Dil &amp; Runtime, B3 Veri Erişimi (ADR-0027).
    /// </summary>
    /// <remarks>
    /// Was <c>DomainId</c>, which answered two questions at once: `backend` was an area, `security` was a
    /// line inside it, and the column meant whichever the row happened to be.
    ///
    /// The AREA is not stored here. It is the line's area — asked through the join, never duplicated, so a
    /// topic cannot end up on the B3 line while claiming to be in Frontend.
    /// </remarks>
    public required Guid LineId { get; set; }

    public required TopicCategory Category { get; set; }

    /// <summary>
    /// The SHAPE of the explanation (ADR-0024): Mechanism, Comparison, Incident… It decides which blocks the
    /// editor's skeleton starts with. Orthogonal to <see cref="Category"/> — that is the subject, this is the
    /// form. Defaulted so existing rows have a value; the authoring flow sets it deliberately.
    /// </summary>
    public Archetype Archetype { get; set; } = Archetype.Concept;

    /// <summary>
    /// The SCOPE — the neighbourhood of 3-10 stops this one belongs to, or null (ADR-0027).
    /// </summary>
    /// <remarks>
    /// Orthogonal to <see cref="Category"/> and <see cref="DefaultLevel"/>: Category is what KIND of topic
    /// it is, this is which neighbourhood of the line it stands in. Null is normal — a standalone stop
    /// belongs to no neighbourhood, and that is a fact rather than a gap.
    ///
    /// Was <c>ScopeId</c> (ADR-0023's theme). ADR-0027 found the two were one axis under two names.
    /// </remarks>
    public Guid? ScopeId { get; set; }

    /// <summary>
    /// The same four levels a learner states about themselves (`07` — TopicLevels: "These levels are
    /// permanent product concepts"). One enum, not two: a topic written for a Mid-Level reader and a reader
    /// who calls themselves Mid-Level must mean the same thing, or the roadmap cannot match them.
    /// </summary>
    public required SkillLevel DefaultLevel { get; set; }

    /// <summary>The canonical (English) title, so a topic can be listed without loading a translation.</summary>
    public required string DefaultTitle { get; set; }

    /// <summary>
    /// Where this stop sits in a numbered chain: Change Tracking I / II / III. Null for most (ADR-0027).
    /// </summary>
    /// <remarks>
    /// A subject that will not fit in 20-25 minutes is not compressed — it is split. Three finishable stops
    /// beat one 45-minute page, for the reading and for the streak alike.
    ///
    /// An owned value, not three loose columns: `part` without `of` is meaningless, and a shape that can
    /// express "2 of null" is a shape somebody will eventually store.
    /// </remarks>
    public TopicSequence? Sequence { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Line? Line { get; init; }
    public Scope? Scope { get; init; }
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

/// <summary>
/// A stop's place in a numbered chain: "Change Tracking II / III" (ADR-0027).
/// </summary>
/// <remarks>
/// <c>Group</c> is what ties the chain together — the parts share it. The prerequisite edges (I → II → III)
/// already exist as relationships; this is what the badge beside the title reads from, and what a
/// "kapsam tamamlandı" celebration counts.
/// </remarks>
public sealed record TopicSequence(string Group, int Part, int Of);
