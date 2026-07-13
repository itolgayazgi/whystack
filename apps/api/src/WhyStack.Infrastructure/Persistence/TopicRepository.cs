using Microsoft.EntityFrameworkCore;
using WhyStack.Application.Content;
using WhyStack.Domain.Content;

namespace WhyStack.Infrastructure.Persistence;

public sealed class TopicRepository(WhyStackDbContext context) : ITopicRepository
{
    /// <summary>
    /// What a reader who is not an editor may see.
    /// </summary>
    /// <remarks>
    /// `04`: "Draft content is not publicly accessible." Everything before Published is somebody's work in
    /// progress — a model's draft, a half-finished review. Deprecated stays visible, deliberately: a reader
    /// who followed a link to an old topic is better served by the old topic and a warning than by a 404.
    /// </remarks>
    private static readonly ContentStatus[] PubliclyVisible = [ContentStatus.Published, ContentStatus.Deprecated];

    public async Task<Page<TopicRecord>> ListAsync(TopicQuery query, CancellationToken cancellationToken)
    {
        var topics = Readable(query.IncludeDrafts);

        if (!string.IsNullOrWhiteSpace(query.Technology))
        {
            topics = topics.Where(topic => topic.Technology == query.Technology);
        }

        if (!string.IsNullOrWhiteSpace(query.Level) && Enum.TryParse<Domain.Users.SkillLevel>(query.Level, true, out var level))
        {
            topics = topics.Where(topic => topic.DefaultLevel == level);
        }

        // COUNT before Skip/Take. Doing it after would count the page, and the pagination metadata would
        // tell every client there is exactly one page — forever.
        var total = await topics.CountAsync(cancellationToken);

        var page = await topics
            .OrderBy(topic => topic.Technology)
            .ThenBy(topic => topic.DefaultLevel)
            .ThenBy(topic => topic.DefaultTitle)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(Projection())
            .ToListAsync(cancellationToken);

        return new Page<TopicRecord>(page, query.PageNumber, query.PageSize, total);
    }

    public async Task<TopicRecord?> FindBySlugAsync(
        string slug,
        bool includeDrafts,
        CancellationToken cancellationToken) =>
        await Readable(includeDrafts)
            .Where(topic => topic.Slug == slug)
            .Select(Projection())
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, TopicLink>> LinksForAsync(
        IReadOnlyCollection<Guid> topicIds,
        string language,
        CancellationToken cancellationToken)
    {
        // A graph edge may point at a topic that is not published yet. The edge stays in the database — the
        // author declared it and it is true — but the reader is not offered a link to a page they cannot
        // open. It simply does not appear, which is what a link to nothing should do.
        var links = await Readable(includeDrafts: false)
            .Where(topic => topicIds.Contains(topic.Id))
            .Select(topic => new
            {
                topic.Id,
                topic.StableKey,
                topic.Slug,
                topic.DefaultTitle,

                // One query, not one per topic. The translated title comes back in the same round trip; an
                // N+1 here would turn a twenty-item "related topics" list into twenty-one queries on the
                // page a reader waits longest for.
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

    private IQueryable<Topic> Readable(bool includeDrafts)
    {
        var topics = context.Topics.AsNoTracking().Where(topic => topic.IsActive);

        return includeDrafts
            ? topics
            : topics.Where(topic => topic.Versions
                .Any(version => PubliclyVisible.Contains(version.Status)));
    }

    /// <summary>
    /// One projection, used by every read.
    /// </summary>
    /// <remarks>
    /// A projection rather than <c>Include</c>: EF translates this to a single query with the joins it
    /// actually needs, and it never materialises a Topic entity nobody asked for. It also means the columns
    /// this API depends on are written down in exactly one place — add a field to <see cref="TopicRecord"/>
    /// and the compiler shows you the only line that has to change.
    /// </remarks>
    private static System.Linq.Expressions.Expression<Func<Topic, TopicRecord>> Projection() =>
        topic => new TopicRecord(
            topic.Id,
            topic.StableKey,
            topic.Slug,
            topic.Technology,
            topic.Category.ToString(),
            topic.DefaultLevel.ToString(),

            topic.Versions.OrderByDescending(version => version.VersionNumber)
                .Select(version => version.Status.ToString()).First(),

            topic.DefaultTitle,

            topic.Versions.OrderByDescending(version => version.VersionNumber)
                .Select(version => version.CanonicalLanguageCode).First(),

            topic.Versions.OrderByDescending(version => version.VersionNumber)
                .Select(version => version.MarkdownPath).First(),

            topic.Versions.OrderByDescending(version => version.VersionNumber)
                .Select(version => version.ContentHash).First(),

            topic.Versions.OrderByDescending(version => version.VersionNumber)
                .Select(version => version.EstimatedReadingMinutes).First(),

            topic.Versions.OrderByDescending(version => version.VersionNumber)
                .Select(version => version.LastReviewedOn).First(),

            topic.Versions.OrderByDescending(version => version.VersionNumber)
                .SelectMany(version => version.SupportedVersions)
                .Select(supported => supported.Version)
                .ToList(),

            topic.Versions.OrderByDescending(version => version.VersionNumber)
                .SelectMany(version => version.Sections)
                .OrderBy(section => section.SortOrder)
                .Select(section => section.SectionTypeKey)
                .ToList(),

            topic.Versions.OrderByDescending(version => version.VersionNumber)
                .SelectMany(version => version.Translations)
                .Select(translation => new TopicTranslationRecord(
                    translation.LanguageCode,
                    translation.Title,
                    translation.MarkdownPath,
                    translation.ContentHash))
                .ToList(),

            topic.OutgoingRelationships
                .Select(edge => new TopicEdge(edge.Type.ToString(), edge.ToTopicId))
                .ToList());
}
