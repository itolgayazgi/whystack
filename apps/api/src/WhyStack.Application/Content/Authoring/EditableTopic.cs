namespace WhyStack.Application.Content.Authoring;

/// <summary>
/// A topic as the STUDIO needs it — everything, in every language, in every ecosystem.
/// </summary>
/// <remarks>
/// Deliberately not <see cref="TopicDetail"/>. That one is what a READER gets: one language, one
/// implementation panel opened, graph edges resolved into clickable links. An editor needs the opposite —
/// both languages side by side (that is how you notice a translation has drifted), every implementation at
/// once, and the graph as raw stable keys, because they are about to change them.
///
/// Two shapes for two jobs. Folding them into one would mean the reading endpoint carries fields no reader
/// needs and the studio guesses at fields it was not given.
/// </remarks>
public sealed record EditableTopic(
    Guid Id,
    string StableKey,
    string Slug,
    string DomainKey,

    /// <summary>The theme key, or null (ADR-0023). The editor picks it from the catalog's SubArea list.</summary>
    string? SubAreaKey,

    string Category,
    string Level,
    string Status,
    int EstimatedReadingMinutes,
    DateOnly LastReviewedOn,
    IReadOnlyList<string> SupportedVersions,
    IReadOnlyList<EditableTranslation> Translations,
    IReadOnlyList<EditableSection> Sections,
    IReadOnlyList<EditableImplementation> Implementations,
    IReadOnlyList<EditableRelationship> Relationships,

    /// <summary>What the editor must send back on save, so a second tab cannot silently revert this one.</summary>
    string RowVersion,

    /// <summary>
    /// What is wrong with it right now.
    /// </summary>
    /// <remarks>
    /// Returned with the DRAFT, not instead of it. A draft is allowed to be incomplete — an author saves a
    /// half-written topic twenty times an hour — and an editor cannot fix what they cannot open. The
    /// problems are the to-do list, not a rejection.
    /// </remarks>
    IReadOnlyList<Validation.ContentProblem> Problems);

public sealed record EditableTranslation(string LanguageCode, string Title, string? Summary, string Status);

public sealed record EditableSection(string SectionTypeKey, string LanguageCode, string Markdown);

public sealed record EditableImplementation(
    string EcosystemKey,
    string? ProgrammingLanguageKey,
    string SupportedVersions,
    IReadOnlyList<EditableSection> Sections);

/// <summary>
/// A graph edge, as a STABLE KEY rather than a resolved link.
/// </summary>
/// <remarks>
/// The reader gets a title and a slug, because they are going to click it. The editor gets the key, because
/// they are going to change it — and a slug can be corrected while a stable key cannot, so the thing an
/// editor edits must be the thing that is stored.
/// </remarks>
public sealed record EditableRelationship(string Type, string ToStableKey, string ToTitle);

/// <summary>One row in the studio's list. Every topic, at every stage — this is the editor's workbench.</summary>
public sealed record StudioTopic(
    Guid Id,
    string StableKey,
    string Slug,
    string Title,
    string DomainName,

    /// <summary>The theme's name, or null. A topic with no thread shows a dash, not a blank.</summary>
    string? SubAreaName,

    string Level,
    string Status,
    DateTime? UpdatedAtUtc,

    /// <summary>Which languages have any text at all. An editor's first question about a topic.</summary>
    IReadOnlyList<string> Languages,

    /// <summary>Which ecosystems have an implementation. Empty is normal — a concept with no code.</summary>
    IReadOnlyList<string> Ecosystems);

/// <summary>An approved technical term. The term is preserved; only its explanation is translated.</summary>
public sealed record EditableTerm(
    Guid Id,
    string Text,
    IReadOnlyList<string> Aliases,
    IReadOnlyList<string> ForbiddenTranslations,
    IReadOnlyList<TermExplanationModel> Explanations);

public sealed record TermExplanationModel(string LanguageCode, string Text);

public sealed record SaveTermCommand(
    Guid? Id,
    string Text,
    IReadOnlyList<string> Aliases,
    IReadOnlyList<string> ForbiddenTranslations,
    IReadOnlyList<TermExplanationModel> Explanations);

/// <summary>A theme, as the studio manages it (ADR-0023). One row: a stable key and a display name.</summary>
public sealed record EditableSubArea(Guid Id, string Key, string Name, int TopicCount);

public sealed record SaveSubAreaCommand(Guid? Id, string Key, string Name);

/// <summary>
/// The result of trying to delete a theme.
/// </summary>
/// <remarks>
/// A theme in use cannot be deleted (ADR-0023): the foreign key is Restrict, because deleting it would
/// silently untag every topic that carried it. <c>InUseCount</c> is how the editor is told what stands in
/// the way — a number, so the message can say "used by 7 topics" rather than "cannot delete".
/// </remarks>
public sealed record DeleteSubAreaOutcome(bool Deleted, int InUseCount);
