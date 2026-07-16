using WhyStack.Application.Common;

namespace WhyStack.Application.Roadmap;

/// <summary>One ecosystem's line through one domain, with this reader's state on it.</summary>
public sealed class GetRoadmapHandler(IRoadmapRepository repository)
{
    public async Task<Result<RoadmapView>> HandleAsync(
        Guid userId,
        string ecosystemKey,
        string domainKey,
        string language,
        CancellationToken cancellationToken)
    {
        var roadmap = await repository.GetAsync(userId, ecosystemKey, domainKey, language, cancellationToken);

        return roadmap is null
            ? new Error(ErrorCodes.ResourceNotFound, "No such ecosystem, or no such domain.")
            : Result<RoadmapView>.Success(roadmap);
    }
}

/// <summary>The ecosystem tabs, unavailable ones included — the design shows them as "YAKINDA".</summary>
public sealed class GetLinesHandler(IRoadmapRepository repository)
{
    public Task<IReadOnlyList<LineOption>> HandleAsync(CancellationToken cancellationToken) =>
        repository.LinesAsync(cancellationToken);
}
