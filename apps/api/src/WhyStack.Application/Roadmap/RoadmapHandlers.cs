using WhyStack.Application.Common;

namespace WhyStack.Application.Roadmap;

/// <summary>One ecosystem's line through one domain, with this reader's state on it.</summary>
public sealed class GetRoadmapHandler(IRoadmapRepository repository)
{
    public async Task<Result<RoadmapView>> HandleAsync(
        Guid userId,
        string ecosystemKey,
        string lineKey,
        string language,
        CancellationToken cancellationToken)
    {
        var roadmap = await repository.GetAsync(userId, ecosystemKey, lineKey, language, cancellationToken);

        return roadmap is null
            ? new Error(ErrorCodes.ResourceNotFound, "No such ecosystem, or no such domain.")
            : Result<RoadmapView>.Success(roadmap);
    }
}

/// <summary>The ecosystem tabs, unavailable ones included — the design shows them as "YAKINDA".</summary>
public sealed class GetEcosystemsHandler(IRoadmapRepository repository)
{
    public Task<IReadOnlyList<EcosystemOption>> HandleAsync(string areaKey, CancellationToken cancellationToken) =>
        repository.EcosystemsAsync(areaKey, cancellationToken);
}

/// <summary>The sidebar's area rail: Backend, Frontend, Database, DevOps.</summary>
public sealed class GetAreasHandler(IRoadmapRepository repository)
{
    public Task<IReadOnlyList<AreaOption>> HandleAsync(CancellationToken cancellationToken) =>
        repository.AreasAsync(cancellationToken);
}

/// <summary>
/// The lines inside one area — B1..B8 for Backend.
/// </summary>
/// <remarks>
/// An empty list is a real answer, not a 404: Frontend exists and has no lines written yet (ADR-0027). The
/// sidebar shows the area with nothing under it, which is the truth.
/// </remarks>
public sealed class GetLinesHandler(IRoadmapRepository repository)
{
    public Task<IReadOnlyList<LineOption>> HandleAsync(string areaKey, CancellationToken cancellationToken) =>
        repository.LinesAsync(areaKey, cancellationToken);
}
