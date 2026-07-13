using System.Text.Json.Serialization;

namespace WhyStack.ContentImport;

/// <summary>
/// The validated content manifest, written by <c>pnpm content:validate</c>.
/// </summary>
/// <remarks>
/// This importer reads THIS and nothing else. It never opens <c>content/</c>, and it re-implements not one
/// validation rule — the rules live once, in <c>@whystack/knowledge-engine</c> (ADR-0018).
///
/// The manifest is written only when the corpus passes. So invalid content cannot reach the database, and
/// not because this program is careful: the file it needs does not exist.
///
/// The Markdown is not here. `07`: "Markdown may exist in files. The database stores metadata,
/// relationships and publishing state." What travels is the path and the hash.
/// </remarks>
public sealed record ContentManifest(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("topics")] IReadOnlyList<ManifestTopic> Topics);

public sealed record ManifestTopic(
    [property: JsonPropertyName("stableKey")] string StableKey,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("technology")] string Technology,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("level")] string Level,
    [property: JsonPropertyName("defaultTitle")] string DefaultTitle,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("lastReviewed")] string LastReviewed,
    [property: JsonPropertyName("estimatedReadingMinutes")] int EstimatedReadingMinutes,
    [property: JsonPropertyName("supportedVersions")] IReadOnlyList<string> SupportedVersions,
    [property: JsonPropertyName("canonicalLanguage")] string CanonicalLanguage,
    [property: JsonPropertyName("canonicalMarkdownPath")] string CanonicalMarkdownPath,
    [property: JsonPropertyName("canonicalContentHash")] string CanonicalContentHash,
    [property: JsonPropertyName("sections")] IReadOnlyList<string> Sections,
    [property: JsonPropertyName("relationships")] IReadOnlyList<ManifestRelationship> Relationships,
    [property: JsonPropertyName("translations")] IReadOnlyList<ManifestTranslation> Translations);

public sealed record ManifestRelationship(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("topic")] string Topic);

public sealed record ManifestTranslation(
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("markdownPath")] string MarkdownPath,
    [property: JsonPropertyName("contentHash")] string ContentHash);
