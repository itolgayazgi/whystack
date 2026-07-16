using Microsoft.EntityFrameworkCore;
using WhyStack.Application.Roadmap;
using WhyStack.Domain.Content;

namespace WhyStack.Infrastructure.Persistence;

public sealed class RoadmapRepository(WhyStackDbContext context) : IRoadmapRepository
{
    public async Task<IReadOnlyList<LineOption>> LinesAsync(CancellationToken cancellationToken) =>
        await context.Ecosystems
            .AsNoTracking()
            .OrderBy(ecosystem => ecosystem.SortOrder)
            .Select(ecosystem => new LineOption(
                ecosystem.Key,
                ecosystem.Name,
                ecosystem.IsAvailable,

                // Counted, not assumed. "Available" is the product's intent; the count is the truth, and a
                // tab that promises a line with nothing on it is a tab that wastes a click.
                context.Topics.Count(topic =>
                    topic.Versions
                        .OrderByDescending(version => version.VersionNumber)
                        .Select(version => version.Status)
                        .First() == ContentStatus.Published
                    && topic.Versions
                        .OrderByDescending(version => version.VersionNumber)
                        .First()
                        .Implementations
                        .Any(implementation => implementation.Ecosystem!.Key == ecosystem.Key))))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DomainOption>> DomainsAsync(CancellationToken cancellationToken) =>
        await context.KnowledgeDomains
            .AsNoTracking()
            .OrderBy(domain => domain.SortOrder)
            .Select(domain => new DomainOption(
                domain.Key,
                domain.Name,
                context.Topics.Count(topic =>
                    topic.DomainId == domain.Id
                    && topic.Versions
                        .OrderByDescending(version => version.VersionNumber)
                        .Select(version => version.Status)
                        .First() == ContentStatus.Published)))
            .ToListAsync(cancellationToken);

    public async Task<RoadmapView?> GetAsync(
        Guid userId,
        string ecosystemKey,
        string domainKey,
        string language,
        CancellationToken cancellationToken)
    {
        var line = await context.Ecosystems
            .AsNoTracking()
            .Where(ecosystem => ecosystem.Key == ecosystemKey)
            .Select(ecosystem => new { ecosystem.Key, ecosystem.Name })
            .SingleOrDefaultAsync(cancellationToken);

        var domain = await context.KnowledgeDomains
            .AsNoTracking()
            .Where(candidate => candidate.Key == domainKey)
            .Select(candidate => new { candidate.Id, candidate.Key, candidate.Name })
            .SingleOrDefaultAsync(cancellationToken);

        // Refused rather than answered with an empty line. An empty map and "no such ecosystem" look
        // identical to a reader, and only one of them is worth telling them about.
        if (line is null || domain is null) return null;

        var stations = await context.Topics
            .AsNoTracking()
            .Where(topic =>
                topic.DomainId == domain.Id
                && topic.Versions
                    .OrderByDescending(version => version.VersionNumber)
                    .Select(version => version.Status)
                    .First() == ContentStatus.Published)

            // The station ORDER. Level first — that is the basamak the whole product is built on — then the
            // theme's own order (ADR-0023), then the title so the sequence is STABLE. Not a graph walk over
            // `Next` edges: those edges are sparse today, and a topological sort over a sparse graph produces
            // a confident-looking order that changes whenever an editor adds one edge. A wrong-but-stable
            // line is honest; a line that reshuffles under the reader is not.
            .OrderByLevel()
            .ThenBy(topic => topic.SubArea!.SortOrder)
            .ThenBy(topic => topic.DefaultTitle)
            .Select(topic => new
            {
                topic.Id,
                topic.Slug,
                topic.DefaultTitle,
                topic.DefaultLevel,
                SubAreaName = topic.SubArea!.Name,

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
                        && edge.ToTopic!.DomainId != domain.Id
                        && edge.ToTopic.Versions
                            .OrderByDescending(version => version.VersionNumber)
                            .Select(version => version.Status)
                            .First() == ContentStatus.Published)
                    .Select(edge => new TransferView(
                        edge.ToTopic!.Slug,
                        edge.ToTopic.DefaultTitle,
                        edge.ToTopic.Domain!.Name))
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
                    station.SubAreaName,
                    station.Version.EstimatedReadingMinutes,
                    state,
                    percent,
                    station.Transfer);
            })
            .ToList();

        return new RoadmapView(line.Key, line.Name, domain.Key, domain.Name, views);
    }
}
