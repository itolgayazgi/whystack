using Microsoft.EntityFrameworkCore;
using WhyStack.Application.Roadmap;
using WhyStack.Domain.Content;

namespace WhyStack.Infrastructure.Persistence;

public sealed class RoadmapRepository(WhyStackDbContext context) : IRoadmapRepository
{
    /// <summary>Published stops, whichever line they are on. The shared filter for every count below.</summary>
    private IQueryable<Topic> Published() =>
        context.Topics.Where(topic =>
            topic.Versions
                .OrderByDescending(version => version.VersionNumber)
                .Select(version => version.Status)
                .First() == ContentStatus.Published);

    public async Task<IReadOnlyList<EcosystemOption>> EcosystemsAsync(
        string areaKey,
        CancellationToken cancellationToken) =>
        await context.Ecosystems
            .AsNoTracking()
            .Where(ecosystem => ecosystem.Area!.Key == areaKey)
            .OrderBy(ecosystem => ecosystem.SortOrder)
            .Select(ecosystem => new EcosystemOption(
                ecosystem.Key,
                ecosystem.Name,
                ecosystem.IsAvailable,

                // Counted, not assumed. "Available" is the product's intent; the count is the truth, and a
                // tab that promises a network with nothing on it is a tab that wastes a click.
                Published().Count(topic =>
                    topic.Versions
                        .OrderByDescending(version => version.VersionNumber)
                        .First()
                        .Implementations
                        .Any(implementation => implementation.Ecosystem!.Key == ecosystem.Key))))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AreaOption>> AreasAsync(CancellationToken cancellationToken) =>
        await context.Areas
            .AsNoTracking()
            .OrderBy(area => area.SortOrder)
            .Select(area => new AreaOption(
                area.Key,
                area.Name,

                // Through the LINE, not from the topic. A topic has no area of its own (ADR-0027) — it has a
                // line, and the line has an area. Storing it twice is how the two drift apart.
                Published().Count(topic => topic.Line!.AreaId == area.Id)))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LineOption>> LinesAsync(string areaKey, CancellationToken cancellationToken) =>
        await context.Lines
            .AsNoTracking()
            .Where(line => line.Area!.Key == areaKey)
            .OrderBy(line => line.SortOrder)
            .Select(line => new LineOption(
                line.Key,
                line.Name,
                line.Color,
                Published().Count(topic => topic.LineId == line.Id)))
            .ToListAsync(cancellationToken);

    public async Task<RoadmapView?> GetAsync(
        Guid userId,
        string ecosystemKey,
        string lineKey,
        string language,
        CancellationToken cancellationToken)
    {
        var ecosystem = await context.Ecosystems
            .AsNoTracking()
            .Where(candidate => candidate.Key == ecosystemKey)
            .Select(candidate => new { candidate.Key, candidate.Name })
            .SingleOrDefaultAsync(cancellationToken);

        var line = await context.Lines
            .AsNoTracking()
            .Where(candidate => candidate.Key == lineKey)
            .Select(candidate => new { candidate.Id, candidate.Key, candidate.Name, candidate.Color, candidate.AreaId })
            .SingleOrDefaultAsync(cancellationToken);

        // Refused rather than answered with an empty map. An empty line and "no such line" look identical to
        // a reader, and only one of them is worth telling them about.
        if (ecosystem is null || line is null) return null;

        var stations = await context.Topics
            .AsNoTracking()
            .Where(topic =>
                topic.LineId == line.Id
                && topic.Versions
                    .OrderByDescending(version => version.VersionNumber)
                    .Select(version => version.Status)
                    .First() == ContentStatus.Published)

            // The station ORDER. Level first — that is the basamak the whole product is built on — then the
            // theme's own order (ADR-0023), then the title so the sequence is STABLE. Not a graph walk over
            // `Next` edges: those edges are sparse today, and a topological sort over a sparse graph produces
            // a confident-looking order that changes whenever an editor adds one edge. A wrong-but-stable
            // line is honest; a line that reshuffles under the reader is not.
            .OrderBy(topic => topic.DefaultLevel)
            .ThenBy(topic => topic.Scope!.SortOrder)
            .ThenBy(topic => topic.DefaultTitle)
            .Select(topic => new
            {
                topic.Id,
                topic.Slug,
                topic.DefaultTitle,
                topic.DefaultLevel,
                ScopeKey = topic.Scope!.Key,
                ScopeName = topic.Scope.Name,
                topic.Sequence,

                Version = topic.Versions
                    .OrderByDescending(version => version.VersionNumber)
                    .Select(version => new
                    {
                        version.EstimatedReadingMinutes,
                        version.CanonicalLanguageCode,
                        Title = version.Translations
                            .Where(translation => translation.LanguageCode == language)
                            .Select(translation => translation.Title)
                            .FirstOrDefault(),
                        Blocks = version.Blocks.Count(block =>
                            block.LanguageCode == version.CanonicalLanguageCode
                            && (block.EcosystemKey == null || block.EcosystemKey == ecosystemKey)),
                    })
                    .First(),

                Progress = context.UserTopicProgress
                    .Where(progress =>
                        progress.UserId == userId
                        && progress.TopicId == topic.Id
                        && (progress.EcosystemKey == null || progress.EcosystemKey == ecosystemKey))
                    .OrderByDescending(progress => progress.UpdatedAtUtc)
                    .Select(progress => new { progress.LastBlockOrder, progress.IsCompleted })
                    .FirstOrDefault(),

                // The transfer: a Related edge that leaves this domain. Inside the domain it is just the next
                // station, and a symbol that marks everything marks nothing.
                Transfer = topic.OutgoingRelationships
                    .Where(edge =>
                        edge.Type == RelationshipType.Related
                        && edge.ToTopic!.Line!.AreaId != line.AreaId
                        && edge.ToTopic.Versions
                            .OrderByDescending(version => version.VersionNumber)
                            .Select(version => version.Status)
                            .First() == ContentStatus.Published)
                    .Select(edge => new TransferView(
                        edge.ToTopic!.Slug,
                        edge.ToTopic.DefaultTitle,
                        edge.ToTopic.Line!.Area!.Name))
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        // The state is decided HERE, over the whole ordered line, because "current" and "next" are facts
        // about the line rather than about any one topic. Deciding them per-row in SQL would need the row to
        // know what came before it, which is exactly the thing a row does not know.
        var currentIndex = stations.FindIndex(station =>
            station.Progress is { IsCompleted: false, LastBlockOrder: > 0 });

        var nextIndex = stations.FindIndex(station => station.Progress is null);

        var views = stations
            .Select((station, index) =>
            {
                var state = station.Progress?.IsCompleted == true
                    ? StationState.Done
                    : index == currentIndex
                        ? StationState.Current
                        : index == nextIndex && currentIndex != nextIndex
                            ? StationState.Next
                            : StationState.Ahead;

                var percent = station.Progress is null || station.Version.Blocks == 0
                    ? 0
                    : station.Progress.IsCompleted
                        ? 100
                        : Math.Clamp(station.Progress.LastBlockOrder * 100 / station.Version.Blocks, 0, 100);

                return new StationView(
                    station.Slug,
                    station.Version.Title ?? station.DefaultTitle,
                    station.DefaultLevel.ToString(),
                    station.ScopeKey,
                    station.ScopeName,
                    station.Version.EstimatedReadingMinutes,
                    state,
                    percent,
                    station.Sequence is null
                        ? null
                        : new SequenceView(station.Sequence.Group, station.Sequence.Part, station.Sequence.Of),
                    station.Transfer);
            })
            .ToList();

        return new RoadmapView(ecosystem.Key, ecosystem.Name, line.Key, line.Name, line.Color, views);
    }
}
