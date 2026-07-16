using Microsoft.EntityFrameworkCore;
using WhyStack.Application.Content;
using Blocks = WhyStack.Application.Content.Blocks;
using WhyStack.Application.Content.Validation;
using WhyStack.Domain.Content;

namespace WhyStack.Infrastructure.Persistence;

public sealed class TopicRepository(WhyStackDbContext context) : ITopicRepository
{
    /// <summary>
    /// What a reader who is not an editor may see.
    /// </summary>
    /// <remarks>
    /// `04`: "Draft content is not publicly accessible." Everything before Published is somebody's work in
    /// progress. Deprecated stays visible, deliberately: a reader who followed a link to an old topic is
    /// better served by the old topic and a warning than by a 404.
    /// </remarks>
    private static readonly ContentStatus[] PubliclyVisible = [ContentStatus.Published, ContentStatus.Deprecated];

    public async Task<Page<TopicRecord>> ListAsync(TopicQuery query, CancellationToken cancellationToken)
    {
        var topics = Readable(query.IncludeDrafts);

        if (!string.IsNullOrWhiteSpace(query.Line))
        {
            topics = topics.Where(topic => topic.Line!.Key == query.Line);
        }

        if (!string.IsNullOrWhiteSpace(query.Level)
            && Enum.TryParse<Domain.Users.SkillLevel>(query.Level, true, out var level))
        {
            topics = topics.Where(topic => topic.DefaultLevel == level);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Title and summary, across the canonical text AND every translation — a Turkish reader typing
            // "bellek" has to find a topic whose canonical title is English.
            //
            // A LIKE '%term%' scan, and honestly so: it cannot use an index, and at a few hundred topics it
            // does not need to. The moment the corpus makes this slow, the answer is full-text search, not a
            // cleverer LIKE — and the fix belongs here, in one query, rather than in six callers.
            topics = topics.Where(topic =>
                EF.Functions.Like(topic.DefaultTitle, $"%{query.Search}%")
                || topic.Versions.Any(version =>
                    version.Translations.Any(translation =>
                        EF.Functions.Like(translation.Title, $"%{query.Search}%")
                        || (translation.Summary != null
                            && EF.Functions.Like(translation.Summary, $"%{query.Search}%")))));
        }

        // COUNT before Skip/Take. After, it would count the page — and the pagination metadata would tell
        // every client there is exactly one page, forever.
        var total = await topics.CountAsync(cancellationToken);

        // The LIST DOES NOT CARRY THE PROSE, and that is the point of having two shapes.
        //
        // Twenty topics, each with fifteen sections of Markdown, is megabytes of text sent so that a card can
        // show a title. Under ADR-0018 this was free — the text was in a file nobody loaded. Now it is a
        // column, and "SELECT *" on a content table is how a list screen becomes the slowest page in the app.
        var page = await topics
            .OrderBy(topic => topic.Line!.SortOrder)
            .ThenBy(topic => topic.DefaultLevel)
            .ThenBy(topic => topic.DefaultTitle)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(topic => new
            {
                topic.Id,
                topic.StableKey,
                topic.Slug,
                AreaKey = topic.Line!.Area!.Key,
                AreaName = topic.Line.Area.Name,
                LineKey = topic.Line.Key,
                LineName = topic.Line.Name,
                ScopeKey = topic.Scope == null ? null : topic.Scope.Key,
                ScopeName = topic.Scope == null ? null : topic.Scope.Name,
                topic.Category,
                topic.Archetype,
                topic.DefaultLevel,
                topic.DefaultTitle,

                Version = topic.Versions
                    .OrderByDescending(version => version.VersionNumber)
                    .Select(version => new
                    {
                        version.Status,
                        version.CanonicalLanguageCode,
                        version.EstimatedReadingMinutes,
                        version.LastReviewedOn,
                        SupportedVersions = version.SupportedVersions.Select(supported => supported.Version).ToList(),
                        Translations = version.Translations
                            .Select(translation => new
                            {
                                translation.LanguageCode,
                                translation.Title,
                                translation.Summary,
                                translation.Status,
                            })
                            .ToList(),
                    })
                    .First(),
            })
            .ToListAsync(cancellationToken);

        var records = page
            .Select(topic => new TopicRecord(
                topic.Id,
                topic.StableKey,
                topic.Slug,
                topic.AreaKey,
                topic.AreaName,
                topic.LineKey,
                topic.LineName,
                topic.ScopeKey,
                topic.ScopeName,
                topic.Category.ToString(),
                topic.Archetype.ToString(),
                topic.DefaultLevel.ToString(),
                topic.Version.Status.ToString(),
                topic.DefaultTitle,
                topic.Version.CanonicalLanguageCode,
                topic.Version.EstimatedReadingMinutes,
                topic.Version.LastReviewedOn,
                topic.Version.SupportedVersions,
                [],
                [],
                topic.Version.Translations
                    .Select(translation => new TopicTranslationRecord(
                        translation.LanguageCode,
                        translation.Title,
                        translation.Summary,
                        translation.Status.ToString()))
                    .ToList(),
                [],

                // The list shows titles, not bodies — loading every block's JSON to render a card would make
                // the list screen the slowest page in the app.
                []))
            .ToList();

        return new Page<TopicRecord>(records, query.PageNumber, query.PageSize, total);
    }

    public async Task<TopicRecord?> FindBySlugAsync(
        string slug,
        bool includeDrafts,
        CancellationToken cancellationToken)
    {
        var topic = await Readable(includeDrafts)
            .Include(candidate => candidate.Line).ThenInclude(line => line!.Area)
            .Include(candidate => candidate.Scope)
            .Include(candidate => candidate.Versions).ThenInclude(version => version.Blocks)
            .Include(candidate => candidate.OutgoingRelationships)
            .Include(candidate => candidate.Versions).ThenInclude(version => version.Sections)
            .Include(candidate => candidate.Versions).ThenInclude(version => version.Translations)
            .Include(candidate => candidate.Versions).ThenInclude(version => version.SupportedVersions)
            .Include(candidate => candidate.Versions)
                .ThenInclude(version => version.Implementations)
                .ThenInclude(implementation => implementation.Ecosystem)
            .Include(candidate => candidate.Versions)
                .ThenInclude(version => version.Implementations)
                .ThenInclude(implementation => implementation.Sections)
            .SingleOrDefaultAsync(candidate => candidate.Slug == slug, cancellationToken);

        if (topic is null) return null;

        var version = topic.Versions.OrderByDescending(candidate => candidate.VersionNumber).First();

        return new TopicRecord(
            topic.Id,
            topic.StableKey,
            topic.Slug,
            topic.Line!.Area!.Key,
            topic.Line.Area.Name,
            topic.Line.Key,
            topic.Line.Name,
            topic.Scope?.Key,
            topic.Scope?.Name,
            topic.Category.ToString(),
            topic.Archetype.ToString(),
            topic.DefaultLevel.ToString(),
            version.Status.ToString(),
            topic.DefaultTitle,
            version.CanonicalLanguageCode,
            version.EstimatedReadingMinutes,
            version.LastReviewedOn,
            [.. version.SupportedVersions.Select(supported => supported.Version)],

            [.. version.Sections
                .OrderBy(section => section.SortOrder)
                .Select(section => new TopicSectionRecord(
                    section.SectionTypeKey, section.LanguageCode, section.Markdown, section.SortOrder))],

            [.. version.Implementations.Select(implementation => new TopicImplementationRecord(
                implementation.Id,
                implementation.Ecosystem!.Key,
                implementation.Ecosystem.Name,
                null,
                null,
                implementation.SupportedVersions,
                [.. implementation.Sections
                    .OrderBy(section => section.SortOrder)
                    .Select(section => new TopicSectionRecord(
                        section.SectionTypeKey, section.LanguageCode, section.Markdown, section.SortOrder))]))],

            [.. version.Translations.Select(translation => new TopicTranslationRecord(
                translation.LanguageCode,
                translation.Title,
                translation.Summary,
                translation.Status.ToString()))],

            [.. topic.OutgoingRelationships.Select(edge => new TopicEdge(edge.Type.ToString(), edge.ToTopicId))],

            // Every block, in every language and ecosystem. The handler merges the reader's view (ADR-0024) —
            // one place decides the rule rather than two clients that would drift.
            [.. version.Blocks
                .OrderBy(block => block.Order)
                .Select(block => new TopicBlockRecord(
                    block.Order,
                    block.Type.ToString(),
                    block.LanguageCode,
                    block.EcosystemKey,
                    block.DataJson))]);
    }

    /// <summary>
    /// The published stops of a line, in the line's own order — ids only.
    /// </summary>
    /// <remarks>
    /// The filter and the order both come from <see cref="LineOrder"/>, which the line map reads too. That is
    /// the whole reason this is not two lines of LINQ written here: the reader's "durak 4/12" is a promise
    /// about the route the map draws, and a second ORDER BY would let the two drift apart silently.
    /// </remarks>
    public async Task<IReadOnlyList<Guid>> StopsOnLineAsync(
        string lineKey,
        CancellationToken cancellationToken) =>
        await context.Topics
            .AsNoTracking()
            .Where(topic => topic.Line!.Key == lineKey)
            .Published()
            .InLineOrder()
            .Select(topic => topic.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, TopicLink>> LinksForAsync(
        IReadOnlyCollection<Guid> topicIds,
        string language,
        CancellationToken cancellationToken)
    {
        // An edge may point at a topic that is not published yet. The edge stays — the author declared it and
        // it is true — but the reader is not offered a link to a page they cannot open.
        var links = await Readable(includeDrafts: false)
            .Where(topic => topicIds.Contains(topic.Id))
            .Select(topic => new
            {
                topic.Id,
                topic.StableKey,
                topic.Slug,
                topic.DefaultTitle,

                // One query, not one per topic. An N+1 here would turn a twenty-item "related topics" list
                // into twenty-one round trips, on the page a reader waits longest for.
                TranslatedTitle = topic.Versions
                    .OrderByDescending(version => version.VersionNumber)
                    .SelectMany(version => version.Translations)
                    .Where(translation => translation.LanguageCode == language)
                    .Select(translation => translation.Title)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        return links.ToDictionary(
            link => link.Id,
            link => new TopicLink(link.StableKey, link.Slug, link.TranslatedTitle ?? link.DefaultTitle));
    }

    public async Task<AuthoringCatalog> CatalogAsync(CancellationToken cancellationToken)
    {
        // LINES, not areas.
        //
        // A topic is authored onto a line (ADR-0027) and SaveAsync looks the key up in Lines. This read the
        // Areas table and handed the studio "backend" — a key that table has never contained, so every save
        // came back "No line \"backend\"". The rename moved the TYPE and left the SOURCE behind, which is
        // the shape of mistake a compiler cannot see: both tables have a Key and a Name.
        var domains = await context.Lines
            .AsNoTracking()
            .OrderBy(line => line.Area!.SortOrder)
            .ThenBy(line => line.SortOrder)
            .Select(line => new LineOption(line.Key, line.Name, line.Area!.Key, line.Area.Name))
            .ToListAsync(cancellationToken);

        // Ordered by their LINE first, so the dropdown groups the way the taxonomy does.
        var subAreas = await context.Scopes
            .AsNoTracking()
            .OrderBy(scope => scope.Line!.SortOrder)
            .ThenBy(scope => scope.SortOrder)
            .ThenBy(scope => scope.Name)
            .Select(scope => new ScopeOption(scope.Key, scope.Name, scope.Line!.Key))
            .ToListAsync(cancellationToken);

        // From the enums and BlockSkeletons — the one definition. A hardcoded copy in the studio would be a
        // second one, and the two would disagree the day an archetype is added.
        var archetypes = Enum.GetValues<Archetype>()
            .Select(archetype => new ArchetypeOption(
                archetype.ToString(),
                [.. Blocks.BlockSkeletons.For(archetype).Select(block => block.ToString())]))
            .ToList();

        var blockTypes = Enum.GetValues<BlockType>()
            .Select(type => new BlockTypeOption(type.ToString(), Blocks.BlockSkeletons.Mandatory.Contains(type)))
            .ToList();

        var ecosystems = await context.Ecosystems
            .AsNoTracking()
            .OrderBy(ecosystem => ecosystem.SortOrder)
            .Select(ecosystem => new EcosystemOption(
                ecosystem.Key,
                ecosystem.Name,
                ecosystem.IsAvailable,
                ecosystem.Languages
                    .OrderBy(language => language.SortOrder)
                    .Select(language => new LanguageOption(language.Key, language.Name, language.FenceLanguage))
                    .ToList()))
            .ToListAsync(cancellationToken);

        var sections = await context.SectionTypes
            .AsNoTracking()
            .OrderBy(type => type.SortOrder)
            .Select(type => new SectionTypeOption(
                type.Key, type.SortOrder, type.Scope, type.IsMandatory, type.IsGraphDerived))
            .ToListAsync(cancellationToken);

        // Every topic, INCLUDING drafts. An editor relating a new topic to one they are still writing is the
        // normal case, not an edge case. This is the one place drafts are listed to a caller, and the endpoint
        // that serves it is behind the editor roles.
        var topics = await context.Topics
            .AsNoTracking()
            .OrderBy(topic => topic.DefaultTitle)
            .Select(topic => new TopicOption(topic.Id, topic.StableKey, topic.DefaultTitle))
            .ToListAsync(cancellationToken);

        // From the enum, not a query and not a hardcoded list — the one source, so the dropdown cannot drift
        // from what the save will accept.
        var categories = Enum.GetNames<TopicCategory>();

        return new AuthoringCatalog(
            domains, subAreas, categories, archetypes, blockTypes, ecosystems, sections, topics);
    }

    public async Task<IReadOnlyCollection<TerminologyEntry>> TerminologyAsync(CancellationToken cancellationToken)
    {
        var terms = await context.Terms
            .AsNoTracking()
            .Select(term => new { term.Text, term.Aliases, term.ForbiddenTranslations })
            .ToListAsync(cancellationToken);

        return
        [
            .. terms.Select(term => new TerminologyEntry(
                term.Text,
                Split(term.Aliases),
                Split(term.ForbiddenTranslations)))
        ];
    }

    private static IReadOnlyList<string> Split(string value) =>
        value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private IQueryable<Topic> Readable(bool includeDrafts)
    {
        var topics = context.Topics.AsNoTracking().Where(topic => topic.IsActive);

        return includeDrafts
            ? topics
            : topics.Where(topic => topic.Versions.Any(version => PubliclyVisible.Contains(version.Status)));
    }
}
