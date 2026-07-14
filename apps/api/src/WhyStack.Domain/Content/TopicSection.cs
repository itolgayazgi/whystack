namespace WhyStack.Domain.Content;

/// <summary>
/// One section of a topic's CONCEPT, in one language — text and all (ADR-0020).
/// </summary>
/// <remarks>
/// The Markdown is here. Under ADR-0018 it was in a file and this row held a path; ADR-0020 moved the
/// source of truth into the database, because an editor writes content in the application and a file the
/// API cannot write is not a source it can own.
///
/// <b>Concept sections only.</b> `WhyItExists`, `TradeOffs`, `CoreMentalModel` — the ones whose answer does
/// not depend on the language you write it in (ADR-0021). The ecosystem-specific ones live on
/// <see cref="TopicImplementation"/>.
/// </remarks>
public class TopicSection
{
    public Guid Id { get; init; }

    public required Guid TopicVersionId { get; init; }

    /// <summary>A key from the SectionTypes reference table, and only a <see cref="SectionScope.Concept"/> one.</summary>
    public required string SectionTypeKey { get; init; }

    public required string LanguageCode { get; init; }

    /// <summary>Validated on save (`WhyStack.Application/Content/Validation`). Never trusted from a client.</summary>
    public required string Markdown { get; set; }

    public required int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; set; }

    public TopicVersion? TopicVersion { get; init; }
}

/// <summary>
/// The approved sections — a REFERENCE TABLE, deliberately not an enum (ADR-0002, Decision 2).
/// </summary>
/// <remarks>
/// `07`: "The database must never limit which educational sections can exist." A closed enum would mean a
/// migration every time the blueprint in `10` gained a section — and, worse, a temptation to drop a
/// mandated section rather than migrate. A row is cheaper than a schema change, and it cannot lose.
/// </remarks>
public class SectionType
{
    public required string Key { get; init; }

    /// <summary>Position in `10`'s Master Topic Structure. Sections render in the blueprint's order.</summary>
    public required int SortOrder { get; init; }

    /// <summary>
    /// Written once, or written per ecosystem (ADR-0021).
    /// </summary>
    /// <remarks>
    /// This column is the whole decision, expressed in one field. `TradeOffs` is <see cref="SectionScope.Concept"/>
    /// — a pool is a guess in every runtime. `Syntax` is <see cref="SectionScope.Implementation"/> — it is
    /// nothing BUT the language. Get the classification wrong and either the reasoning gets duplicated per
    /// ecosystem (the defect ADR-0021 exists to remove) or a code sample claims to be language-independent.
    /// </remarks>
    public required SectionScope Scope { get; init; }

    /// <summary>
    /// Rendered from Knowledge Graph edges rather than written by hand (ADR-0002, Decision 5):
    /// Prerequisites, RelatedTopics, NextRecommendedTopic. Stored once as relationships; never as prose.
    /// </summary>
    public required bool IsGraphDerived { get; init; }

    /// <summary>The blueprint's minimum for a standard concept topic (`10` § Mandatory Topic Sections).</summary>
    public required bool IsMandatory { get; init; }
}

public enum SectionScope
{
    /// <summary>Written once. The answer does not depend on the language.</summary>
    Concept = 1,

    /// <summary>Written per ecosystem. The answer IS the language.</summary>
    Implementation = 2,
}
