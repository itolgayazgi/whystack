namespace WhyStack.Domain.Content;

/// <summary>
/// One edge of the Knowledge Graph (`07` — TopicRelationships; ADR-0004).
/// </summary>
/// <remarks>
/// `07` Principle 02: "Avoid storing relationship logic only inside Markdown text." A prerequisite
/// written as prose cannot be traversed, cannot be validated, and goes stale the first time a topic is
/// renamed — silently, because prose has no referential integrity. The edge is the fact; the
/// "Prerequisites" section a reader sees is a rendering of it.
///
/// It lives in SQL Server, not a graph database (ADR-0004, Decision 5): there is no measured traversal
/// need, and a second datastore is a second thing to keep consistent.
/// </remarks>
public class TopicRelationship
{
    public Guid Id { get; init; }

    public required Guid FromTopicId { get; init; }
    public required Guid ToTopicId { get; init; }

    public required RelationshipType Type { get; init; }

    public Topic? FromTopic { get; init; }
    public Topic? ToTopic { get; init; }
}

/// <summary>The canonical edge types (`10`, ADR-0004 Decision 3).</summary>
public enum RelationshipType
{
    Requires = 1,
    Next = 2,
    Related = 3,
    Alternative = 4,
    Uses = 5,
    UsedBy = 6,
    Improves = 7,
    Affects = 8,
    ReplacedBy = 9,
    DeprecatedBy = 10,
}
