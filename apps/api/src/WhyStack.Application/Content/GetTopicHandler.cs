using System.Text.Json;
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
        var stop = await BuildStopAsync(topic, language.Returned, cancellationToken);

        var ecosystems = await BuildEcosystemsAsync(topic, ecosystem, cancellationToken);

        // The one the blocks below belong to. Null when the topic is entirely shared — the "why" that is true
        // everywhere, which needs no ecosystem to be read.
        var showing = ecosystems.FirstOrDefault(option => option.IsSelected)?.Key;

        return Result<TopicDetail>.Success(new TopicDetail(
            topic.Id,
            topic.StableKey,
            topic.Slug,
            title,
            topic.AreaKey,
            topic.LineKey,
            topic.LineName,
            topic.Category,
            topic.Archetype,
            topic.Level,
            topic.SupportedVersions,
            topic.EstimatedReadingMinutes,
            topic.Status,
            topic.LastReviewedOn,
            language,
            sections,
            Implementations(topic, language, ecosystem),
            ecosystems,
            graph,
            stop,
            BlocksIn(topic.Blocks, language.Returned, showing)));
    }

    /// <summary>
    /// The reader's flow: the SHARED blocks plus the chosen ecosystem's, merged by order (ADR-0024).
    /// </summary>
    /// <remarks>
    /// An untagged block is the "why" — written once and true everywhere, so it appears on every line. A
    /// tagged block belongs to one ecosystem's treatment, so a .NET reader never sees Java's state machine.
    /// The merge happens HERE, once, rather than in two clients that would drift.
    ///
    /// A block whose data will not parse is DROPPED rather than thrown: one corrupt row must not take down the
    /// page around it. The save gate (BlockData.Validate) is what stops such a row existing.
    /// </remarks>
    private static IReadOnlyList<TopicBlockView> BlocksIn(
        IReadOnlyList<TopicBlockRecord> blocks,
        string language,
        string? ecosystem)
    {
        var views = new List<TopicBlockView>();

        foreach (var block in blocks
            .Where(candidate => candidate.LanguageCode == language)
            .Where(candidate => candidate.EcosystemKey is null || candidate.EcosystemKey == ecosystem)
            .OrderBy(candidate => candidate.Order))
        {
            try
            {
                using var document = JsonDocument.Parse(block.DataJson);

                views.Add(new TopicBlockView(
                    block.Order, block.Type, block.EcosystemKey, document.RootElement.Clone()));
            }
            catch (JsonException)
            {
                // Skipped, deliberately and visibly in the gap it leaves — not rendered as a broken block.
            }
        }

        return views;
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

        // Blocks are the model (ADR-0024); sections are the retired one and are still consulted only so a
        // topic that has not been converted yet does not report a fallback it is not making.
        var hasTranslation =
            topic.Blocks.Any(block => block.LanguageCode == requested)
            || topic.Sections.Any(section => section.LanguageCode == requested);

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
    /// Which ecosystems this topic's blocks offer, and which one the reader is being shown.
    /// </summary>
    /// <remarks>
    /// <b>A reader who asks for nothing gets the FIRST, not an empty page.</b> That is the decision in this
    /// method, and it is the fix for a real failure: the first topic this project published had every block
    /// tagged `dotnet` — correctly, since "C# neden var?" is .NET's question — and the reader, which sends no
    /// ecosystem until somebody picks one, filtered all six away and rendered "bu konunun içeriği henüz
    /// yazılmadı" over content that was written. A visitor from a search engine has chosen no ecosystem and
    /// never will; refusing to guess for them is not neutrality, it is a blank page.
    ///
    /// First by the product's own SortOrder, not the alphabet. A caller that names an ecosystem still gets
    /// that one — the default only fills a silence.
    ///
    /// The shared blocks come through whatever is selected: they are the "why", true in every ecosystem
    /// (ADR-0021). So a topic with no tagged blocks at all returns an empty list here and reads fine.
    /// </remarks>
    private async Task<IReadOnlyList<TopicEcosystemOption>> BuildEcosystemsAsync(
        TopicRecord topic,
        string? requested,
        CancellationToken cancellationToken)
    {
        var keys = topic.Blocks
            .Select(block => block.EcosystemKey)
            .OfType<string>()
            .Distinct()
            .ToArray();

        var options = await repository.EcosystemsAsync(keys, cancellationToken);

        if (options.Count == 0) return [];

        // The requested one if the topic HAS it; otherwise the first. Asking for "java" on a topic that only
        // has .NET blocks is not an error — it is a reader whose ecosystem this stop was not written for, and
        // the honest answer is the treatment that exists, said out loud by IsSelected.
        var selected = options.Any(option => option.Key == requested) ? requested : options[0].Key;

        return [.. options.Select(option => option with { IsSelected = option.Key == selected })];
    }

    /// <summary>
    /// Where this stop stands on its line, and the stops either side of it.
    /// </summary>
    /// <remarks>
    /// The whole sequence is fetched to find ONE index, then two ids are turned into links. That shape is the
    /// point: ids are cheap, and resolving titles for a line's worth of stops to render two of them would put
    /// a line of translations on the page a reader waits longest for.
    ///
    /// <b>A draft is not on the route.</b> An editor previewing one reaches here with a topic the published
    /// sequence does not contain, so the index is -1 — and the honest answer is null ("this stop has no
    /// number yet"), not stop 0 of 12. Clamping it to a number would print a position the reader would go
    /// looking for and never find, and would quietly renumber the moment it published.
    /// </remarks>
    private async Task<LineStop?> BuildStopAsync(
        TopicRecord topic,
        string language,
        CancellationToken cancellationToken)
    {
        var stops = await repository.StopsOnLineAsync(topic.LineKey, cancellationToken);

        var index = stops.ToList().IndexOf(topic.Id);
        if (index < 0) return null;

        var previousId = index > 0 ? stops[index - 1] : (Guid?)null;
        var nextId = index < stops.Count - 1 ? stops[index + 1] : (Guid?)null;

        Guid[] neighbours = [.. new[] { previousId, nextId }.OfType<Guid>()];

        // One query for both neighbours, not one each. Two round trips on the reading page to render two
        // links is the N+1 in miniature — small enough to never show up in a profile, and paid on every open.
        var links = neighbours.Length == 0
            ? new Dictionary<Guid, TopicLink>()
            : (IReadOnlyDictionary<Guid, TopicLink>)await repository.LinksForAsync(
                neighbours, language, cancellationToken);

        return new LineStop(
            // 1-based: the reader counts "durak 4/12", not "durak 3/12". The design's own chip says so.
            index + 1,
            stops.Count,
            previousId is null ? null : links.GetValueOrDefault(previousId.Value),
            nextId is null ? null : links.GetValueOrDefault(nextId.Value));
    }

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
