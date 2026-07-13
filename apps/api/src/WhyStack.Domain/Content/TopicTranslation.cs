namespace WhyStack.Domain.Content;

/// <summary>
/// A topic version, written in one language (`07` — Localization Domain).
/// </summary>
/// <remarks>
/// `07`: "A translated topic is not a completely separate concept. It is a localized representation of a
/// canonical topic version." So this row carries no identity of its own — no stable key, no level, no
/// graph edges. Those belong to the topic, once. Only the words are per-language.
/// </remarks>
public class TopicTranslation
{
    public Guid Id { get; init; }

    public required Guid TopicVersionId { get; init; }

    public required string LanguageCode { get; init; }

    /// <summary>The title in this language. Titles ARE translated; technical terms inside them are not.</summary>
    public required string Title { get; set; }

    public required string MarkdownPath { get; set; }

    public required string ContentHash { get; set; }

    public required TranslationStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; set; }

    public TopicVersion? TopicVersion { get; init; }
}

/// <summary>`07` — Localization Domain. A translation states how far it has come, and it never lies.</summary>
public enum TranslationStatus
{
    Missing = 1,
    MachineDraft = 2,
    HumanDraft = 3,
    TechnicalReview = 4,
    EditorialReview = 5,
    Approved = 6,
    Published = 7,

    /// <summary>The canonical text moved and this translation has not caught up. It must say so.</summary>
    NeedsUpdate = 8,

    Deprecated = 9,
    Archived = 10,
}
