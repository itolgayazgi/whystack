using WhyStack.Application.Common;

namespace WhyStack.Application.Content;

public sealed class ListTopicsHandler(ITopicRepository repository)
{
    /// <summary>`08` § Pagination Rules. A caller who asks for a thousand rows gets fifty.</summary>
    public const int MaxPageSize = 50;

    public const int DefaultPageSize = 20;

    public async Task<Result<Page<TopicSummary>>> HandleAsync(
        string? domain,
        string? level,
        string requestedLanguage,
        int? pageNumber,
        int? pageSize,
        bool mayReadDrafts,
        CancellationToken cancellationToken)
    {
        // Clamped, not rejected. `pageSize=10000` is far more often a client bug than an attack, and a 422
        // for it teaches nobody anything — while an unbounded query is the thing CLAUDE.md §4 forbids by
        // name. The caller gets fifty rows and the pagination metadata says so.
        var page = Math.Max(1, pageNumber ?? 1);
        var size = Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize);

        var results = await repository.ListAsync(
            new TopicQuery(domain, level, page, size, mayReadDrafts),
            cancellationToken);

        var summaries = results.Items.Select(topic => ToSummary(topic, requestedLanguage)).ToList();

        return Result<Page<TopicSummary>>.Success(
            new Page<TopicSummary>(summaries, results.PageNumber, results.PageSize, results.TotalCount));
    }

    /// <summary>
    /// The list falls back too, and says so PER ROW.
    /// </summary>
    /// <remarks>
    /// Per row, not per response. A Turkish reader's list can hold a translated topic and an untranslated one
    /// at the same time, and one flag for the whole page would have to lie about one of them.
    /// </remarks>
    private static TopicSummary ToSummary(TopicRecord topic, string requestedLanguage)
    {
        var translation = topic.Translations
            .FirstOrDefault(candidate => candidate.Language == requestedLanguage);

        var (title, summary, language) = requestedLanguage == topic.CanonicalLanguage || translation is not null
            ? (translation?.Title ?? topic.DefaultTitle,
               translation?.Summary,
               LanguageResolution.Exact(requestedLanguage))
            : (topic.DefaultTitle,
               topic.Translations.FirstOrDefault(t => t.Language == topic.CanonicalLanguage)?.Summary,
               LanguageResolution.FellBackTo(requestedLanguage, topic.CanonicalLanguage));

        return new TopicSummary(
            topic.Id,
            topic.StableKey,
            topic.Slug,
            title,
            summary,
            topic.DomainKey,
            topic.DomainName,
            topic.Category,
            topic.Level,
            topic.SupportedVersions,
            topic.EstimatedReadingMinutes,
            topic.Status,
            language);
    }
}
