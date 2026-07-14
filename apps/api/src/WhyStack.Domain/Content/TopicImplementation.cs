namespace WhyStack.Domain.Content;

/// <summary>
/// How one ecosystem does the thing the topic explains (ADR-0021, Decision 2).
/// </summary>
/// <remarks>
/// The reader switches between these with the `[ .NET ▾ ]` control. The CONCEPT above it — why the thing
/// exists, what it costs — does not change, because it does not depend on the language. Only this panel
/// does.
///
/// Sections here are the ecosystem-specific ones: `Syntax`, `BasicExample`, `ProgressiveExamples`,
/// `InternalMechanics`, `VersionNotes`. Everything else is written once, on the topic.
///
/// <b>A topic may have none.</b> "What is a transaction?" is a concept with no code, and the panel simply
/// does not render — the same way a graph-derived section with no edges does not render.
/// </remarks>
public class TopicImplementation
{
    public Guid Id { get; init; }

    public required Guid TopicVersionId { get; init; }

    public required Guid EcosystemId { get; init; }

    /// <summary>
    /// The language the code is written in. Nullable: an ecosystem may have one obvious language (.NET → C#)
    /// and an implementation that is about the ecosystem rather than a language within it — a configuration
    /// file, a CLI invocation — has none.
    /// </summary>
    public Guid? ProgrammingLanguageId { get; set; }

    /// <summary>
    /// Which versions of the ECOSYSTEM this implementation is true for: ".NET 8", ".NET 9".
    /// </summary>
    /// <remarks>
    /// Separate from the topic's own versions, because they answer different questions. The topic says
    /// "this concept is current"; the implementation says "this code compiles". A concept outlives the API
    /// that expresses it, and the day `Npgsql` renames a connection-string key, one of these is wrong and
    /// the other is not.
    /// </remarks>
    public required string SupportedVersions { get; set; }

    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; set; }

    public TopicVersion? TopicVersion { get; init; }
    public Ecosystem? Ecosystem { get; init; }

    public ICollection<ImplementationSection> Sections { get; init; } = [];
}

/// <summary>
/// One section of an implementation, and — unlike a concept section — <b>the text is here</b>.
/// </summary>
/// <remarks>
/// ADR-0020 moved the source of truth into the database. There is no Markdown file behind this row: an
/// editor typed it in the app, the API validated it, and it was saved. `content/` is written back OUT of
/// here, as the published record.
/// </remarks>
public class ImplementationSection
{
    public Guid Id { get; init; }

    public required Guid TopicImplementationId { get; init; }

    /// <summary>A key from the SectionTypes reference table — and only an implementation-scoped one.</summary>
    public required string SectionTypeKey { get; init; }

    public required string LanguageCode { get; init; }

    /// <summary>The Markdown. Validated on save; never trusted from the client.</summary>
    public required string Markdown { get; set; }

    public required int SortOrder { get; set; }

    public TopicImplementation? Implementation { get; init; }
}
