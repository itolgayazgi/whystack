namespace WhyStack.Domain.Content;

/// <summary>
/// Which sections a topic version has, and in what order (`07` — TopicSections).
/// </summary>
/// <remarks>
/// Structure, not text. The section BODY lives in the Markdown file; this row says the section exists,
/// what kind it is and where it sits. It is language-independent on purpose: en.md and tr.md are the same
/// topic, so they have the same sections. A translation that dropped one is a defect, and the content
/// validator refuses it before it can ever reach this table.
/// </remarks>
public class TopicSection
{
    public Guid Id { get; init; }

    public required Guid TopicVersionId { get; init; }

    /// <summary>The key from the reference table — "CoreMentalModel", "TradeOffs".</summary>
    public required string SectionTypeKey { get; init; }

    public required int SortOrder { get; init; }

    public TopicVersion? TopicVersion { get; init; }
}

/// <summary>
/// The approved sections — a REFERENCE TABLE, deliberately not an enum (ADR-0002, Decision 2).
/// </summary>
/// <remarks>
/// `07`: "The database must never limit which educational sections can exist." A closed enum would mean
/// a migration every time the blueprint in `10` gained a section — and, worse, a temptation to drop a
/// mandated section rather than migrate. A row is cheaper than a schema change, and it cannot lose.
/// </remarks>
public class SectionType
{
    public required string Key { get; init; }

    /// <summary>Position in `10`'s Master Topic Structure. Renders the sections in the blueprint's order.</summary>
    public required int SortOrder { get; init; }

    /// <summary>
    /// Rendered from Knowledge Graph edges rather than written by hand (ADR-0002 Decision 5):
    /// Prerequisites, RelatedTopics, NextRecommendedTopic. Stored once as relationships; never as prose.
    /// </summary>
    public required bool IsGraphDerived { get; init; }
}
