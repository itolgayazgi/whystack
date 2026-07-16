using WhyStack.Application.Content.Validation;

namespace WhyStack.Application.Content.Authoring;

/// <summary>
/// What the "İçerik Üret" form sends.
/// </summary>
/// <remarks>
/// A FULL REPLACEMENT of the topic's draft, as `08` defines PUT — every section the editor has, every
/// implementation, every relationship. Not a patch.
///
/// A patch API here would look convenient and would be a lie: an editor who deleted a section would have to
/// send a "delete this section" instruction, and the day the client forgot to, the section would live on in
/// the database, in the table of contents, pointing at text nobody wrote. What is on screen is what is
/// saved.
/// </remarks>
public sealed record SaveTopicCommand(
    /// <summary>Null for a new topic. The identity is assigned once and never changes.</summary>
    Guid? Id,

    string StableKey,
    string Slug,
    string LineKey,

    /// <summary>The theme key, or null (ADR-0023). Null is a topic with no thread — normal, not incomplete.</summary>
    string? ScopeKey,

    string Category,
    string Level,
    int EstimatedReadingMinutes,
    IReadOnlyList<string> SupportedVersions,

    /// <summary>Title and summary per language. English is required — a translation with no source cannot be reviewed.</summary>
    IReadOnlyList<TranslationCommand> Translations,

    /// <summary>The shape of the explanation (ADR-0024). Decides which blocks the skeleton starts with.</summary>
    string Archetype,

    /// <summary>
    /// The block flow — the topic's body (ADR-0024).
    /// </summary>
    /// <remarks>
    /// A FULL REPLACEMENT, like everything else here: what is on screen is what is saved. A block the editor
    /// deleted must disappear, not linger in a flow pointing at text nobody wrote.
    /// </remarks>
    IReadOnlyList<BlockCommand> Blocks,

    /// <summary>The concept. Written once, true in every ecosystem (ADR-0021). Retired by ADR-0024.</summary>
    IReadOnlyList<SectionCommand> Sections,

    /// <summary>The `[ .NET ▾ ]` panel. May be empty — "what is a transaction?" has no code.</summary>
    IReadOnlyList<ImplementationCommand> Implementations,

    /// <summary>Knowledge Graph edges. Stored once; never duplicated as prose (ADR-0002, ADR-0004).</summary>
    IReadOnlyList<RelationshipCommand> Relationships,

    /// <summary>
    /// What the editor last read. Absent for a new topic.
    /// </summary>
    /// <remarks>
    /// Content is authored in the application now, so the concurrent-edit problem is ours. Two tabs, two
    /// saves, and without this the second silently discards the first — and nobody is told, because both
    /// requests succeeded.
    /// </remarks>
    string? RowVersion);

public sealed record TranslationCommand(string LanguageCode, string Title, string? Summary);

public sealed record SectionCommand(string SectionTypeKey, string LanguageCode, string Markdown);

/// <summary>One block the editor composed (ADR-0024).</summary>
public sealed record BlockCommand(
    int Order,
    string Type,
    string LanguageCode,

    /// <summary>Null = SHARED (the why, written once). A key = one ecosystem's treatment.</summary>
    string? EcosystemKey,

    /// <summary>The body, shaped by <see cref="Type"/>. Validated against that shape on every save.</summary>
    string DataJson);

public sealed record ImplementationCommand(
    string EcosystemKey,
    string? ProgrammingLanguageKey,
    string SupportedVersions,
    IReadOnlyList<SectionCommand> Sections);

public sealed record RelationshipCommand(string Type, string ToStableKey);

/// <summary>What came back. The problems are the point — an editor fixes them before they publish.</summary>
public sealed record SaveTopicResult(
    Guid Id,
    string Status,
    string RowVersion,
    IReadOnlyList<ContentProblem> Problems);
