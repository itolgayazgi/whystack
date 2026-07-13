using WhyStack.Application.Common;

namespace WhyStack.Application.Content;

/// <summary>One topic, in the reader's language — or in English, and saying so.</summary>
public sealed class GetTopicHandler(ITopicRepository repository, ITopicContentReader content)
{
    public async Task<Result<TopicDetail>> HandleAsync(
        string slug,
        string requestedLanguage,
        bool mayReadDrafts,
        CancellationToken cancellationToken)
    {
        var topic = await repository.FindBySlugAsync(slug, mayReadDrafts, cancellationToken);

        if (topic is null)
        {
            // The same 404 whether the topic does not exist or is an unpublished draft the caller may not
            // read. Distinguishing them would tell an anonymous visitor which topics are being written —
            // which is a content roadmap, and nobody outside is owed it.
            return new Error(ErrorCodes.ResourceNotFound, "No such topic.");
        }

        var (markdownPath, contentHash, title, language) = Resolve(topic, requestedLanguage);

        var sections = await content.ReadSectionsAsync(
            markdownPath, contentHash, topic.SectionTypes, cancellationToken);

        var graph = await BuildGraphAsync(topic, language.Returned, cancellationToken);

        return Result<TopicDetail>.Success(new TopicDetail(
            topic.Id,
            topic.StableKey,
            topic.Slug,
            title,
            topic.Technology,
            topic.Category,
            topic.Level,
            topic.SupportedVersions,
            topic.EstimatedReadingMinutes,
            topic.Status,
            topic.LastReviewedOn,
            language,
            sections,
            graph));
    }

    /// <summary>
    /// Which text to serve, and the honest account of why.
    /// </summary>
    /// <remarks>
    /// A Turkish reader whose topic has no Turkish translation gets the English one — that is better than
    /// an empty page. What they must NOT get is the English one presented as though it were what they
    /// asked for (CLAUDE.md §1.7, `08` Principle 07 — No Silent Fallbacks).
    ///
    /// So the resolution travels with the response, and the client renders it. The fallback is a fact about
    /// the content, not an implementation detail to be tidied away.
    /// </remarks>
    private static (string Path, string Hash, string Title, LanguageResolution Language) Resolve(
        TopicRecord topic,
        string requested)
    {
        if (requested == topic.CanonicalLanguage)
        {
            return (topic.CanonicalMarkdownPath, topic.CanonicalContentHash, topic.DefaultTitle,
                LanguageResolution.Exact(requested));
        }

        var translation = topic.Translations.FirstOrDefault(candidate => candidate.Language == requested);

        if (translation is not null)
        {
            return (translation.MarkdownPath, translation.ContentHash, translation.Title,
                LanguageResolution.Exact(requested));
        }

        return (topic.CanonicalMarkdownPath, topic.CanonicalContentHash, topic.DefaultTitle,
            LanguageResolution.FellBackTo(requested, topic.CanonicalLanguage));
    }

    /// <summary>
    /// The edges, resolved into links a reader can click.
    /// </summary>
    /// <remarks>
    /// One query for every edge of this topic, not one per edge. The N+1 here would be invisible at two
    /// topics and merciless at two thousand — a "related topics" list of twenty would be twenty round
    /// trips, on the page a reader waits longest for.
    /// </remarks>
    private async Task<TopicGraph> BuildGraphAsync(
        TopicRecord topic,
        string language,
        CancellationToken cancellationToken)
    {
        var ids = topic.Edges.Select(edge => edge.ToTopicId).Distinct().ToArray();

        if (ids.Length == 0)
        {
            return new TopicGraph([], [], []);
        }

        var links = await repository.LinksForAsync(ids, language, cancellationToken);

        IReadOnlyList<TopicLink> Of(string type) =>
        [
            .. topic.Edges
                .Where(edge => edge.Type == type)
                .Select(edge => links.GetValueOrDefault(edge.ToTopicId))
                .Where(link => link is not null)
                .Select(link => link!)
        ];

        return new TopicGraph(Of("Requires"), Of("Related"), Of("Next"));
    }
}
