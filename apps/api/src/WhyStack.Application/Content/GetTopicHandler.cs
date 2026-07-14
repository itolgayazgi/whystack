using WhyStack.Application.Common;

namespace WhyStack.Application.Content;

/// <summary>One topic, in the reader's language — or in English, and saying so.</summary>
public sealed class GetTopicHandler(ITopicRepository repository)
{
    public async Task<Result<TopicDetail>> HandleAsync(
        string slug,
        string requestedLanguage,
        string? ecosystem,
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

        var language = Resolve(topic, requestedLanguage);

        var sections = SectionsIn(topic.Sections, language.Returned, topic.CanonicalLanguage);

        var title = topic.Translations
            .FirstOrDefault(translation => translation.Language == language.Returned)?.Title
            ?? topic.DefaultTitle;

        var graph = await BuildGraphAsync(topic, language.Returned, cancellationToken);

        return Result<TopicDetail>.Success(new TopicDetail(
            topic.Id,
            topic.StableKey,
            topic.Slug,
            title,
            topic.DomainKey,
            topic.DomainName,
            topic.Category,
            topic.Level,
            topic.SupportedVersions,
            topic.EstimatedReadingMinutes,
            topic.Status,
            topic.LastReviewedOn,
            language,
            sections,
            Implementations(topic, language, ecosystem),
            graph));
    }

    /// <summary>
    /// Which language to serve, and the honest account of why.
    /// </summary>
    /// <remarks>
    /// A Turkish reader whose topic has no Turkish translation gets the English one — better than an empty
    /// page. What they must NOT get is the English one presented as though it were what they asked for
    /// (CLAUDE.md §1.7, `08` Principle 07 — No Silent Fallbacks). So the resolution travels with the
    /// response, and the client renders it.
    /// </remarks>
    private static LanguageResolution Resolve(TopicRecord topic, string requested)
    {
        if (requested == topic.CanonicalLanguage) return LanguageResolution.Exact(requested);

        var hasTranslation = topic.Sections.Any(section => section.LanguageCode == requested);

        return hasTranslation
            ? LanguageResolution.Exact(requested)
            : LanguageResolution.FellBackTo(requested, topic.CanonicalLanguage);
    }

    private static IReadOnlyList<TopicSectionContent> SectionsIn(
        IReadOnlyList<TopicSectionRecord> sections,
        string language,
        string canonical) =>
        [
            .. sections
                .Where(section => section.LanguageCode == language)
                .OrderBy(section => section.SortOrder)
                .Select(section => new TopicSectionContent(section.SectionTypeKey, section.Markdown))
        ];

    /// <summary>
    /// What sits behind the `[ .NET ▾ ]` control (ADR-0021).
    /// </summary>
    /// <remarks>
    /// ALL of them are returned, not just the one the reader asked for — and that is the design, not
    /// laziness. The concept above the panel is the same page for everybody; only the panel changes, and a
    /// reader who wants to see how Java does it should be able to switch without another round trip. That is
    /// the entire point of teaching the reason first: the reason transfers.
    ///
    /// `ecosystem` says which one to OPEN. It does not filter.
    /// </remarks>
    private static IReadOnlyList<TopicImplementationView> Implementations(
        TopicRecord topic,
        LanguageResolution language,
        string? preferred) =>
        [
            .. topic.Implementations
                .OrderByDescending(implementation => implementation.EcosystemKey == preferred)
                .ThenBy(implementation => implementation.EcosystemName)
                .Select(implementation => new TopicImplementationView(
                    implementation.EcosystemKey,
                    implementation.EcosystemName,
                    implementation.SupportedVersions,
                    implementation.EcosystemKey == preferred,
                    SectionsIn(implementation.Sections, language.Returned, topic.CanonicalLanguage)))
        ];

    /// <summary>
    /// The edges, resolved into links a reader can click.
    /// </summary>
    /// <remarks>
    /// One query for every edge of this topic, not one per edge. The N+1 would be invisible at two topics and
    /// merciless at two thousand — a "related topics" list of twenty would be twenty round trips, on the page
    /// a reader waits longest for.
    /// </remarks>
    private async Task<TopicGraph> BuildGraphAsync(
        TopicRecord topic,
        string language,
        CancellationToken cancellationToken)
    {
        var ids = topic.Edges.Select(edge => edge.ToTopicId).Distinct().ToArray();

        if (ids.Length == 0) return new TopicGraph([], [], []);

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
