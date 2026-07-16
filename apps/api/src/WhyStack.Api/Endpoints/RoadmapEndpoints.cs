using System.Security.Claims;
using WhyStack.Api.Common;
using WhyStack.Application.Roadmap;

namespace WhyStack.Api.Endpoints;

public static class RoadmapEndpoints
{
    public static IEndpointRouteBuilder MapRoadmapEndpoints(this IEndpointRouteBuilder app)
    {
        var roadmap = app
            .MapGroup("/api/v1")
            .WithTags("Roadmap")
            .RequireAuthorization();

        roadmap.MapGet("/roadmap", GetAsync)
            .WithName("GetRoadmap")
            .WithSummary("One ecosystem's line through one domain, with the reader's state on each station.")
            .WithDescription(
                "State is a SUGGESTION, not a gate: 'Ahead' stations are dimmed in the UI and remain fully "
                + "readable. This API enforces no prerequisite, because the product imposes no order.");

        roadmap.MapGet("/ecosystems", EcosystemsAsync)
            .WithName("GetEcosystems")
            .WithSummary("The tabs, including the ones with no content yet.")
            .WithDescription(
                "An ecosystem is the network SWITCHER, not a route through it (ADR-0027). Choosing Java does "
                + "not add a line beside .NET — it rebuilds the same lines in Java.");

        roadmap.MapGet("/areas", AreasAsync)
            .WithName("GetAreas")
            .WithSummary("The sidebar's areas: Backend, Frontend, Database, DevOps.");

        roadmap.MapGet("/areas/{area}/lines", LinesAsync)
            .WithName("GetLines")
            .WithSummary("The lines inside an area — B1..B8 for Backend.")
            .WithDescription(
                "An empty list is a real answer, not a 404: Frontend exists and has no lines written yet.");

        return app;
    }

    private static async Task<IResult> GetAsync(
        ClaimsPrincipal principal,
        GetRoadmapHandler handler,
        HttpContext http,
        CancellationToken cancellationToken,
        string? ecosystem = null,
        string? line = null,
        string? language = null)
    {
        if (string.IsNullOrWhiteSpace(ecosystem) || string.IsNullOrWhiteSpace(line))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Validation failed",
                detail: "Both `ecosystem` and `line` are required — the map is one line, in one ecosystem.");
        }

        var result = await handler.HandleAsync(
            principal.Id(), ecosystem, line, language ?? "en", cancellationToken);

        return result.IsSuccess
            ? Results.Ok(new { data = result.Value, metadata = Metadata(http) })
            : result.Error!.ToProblem(http);
    }

    private static async Task<IResult> EcosystemsAsync(
        GetEcosystemsHandler handler,
        HttpContext http,
        CancellationToken cancellationToken) =>
        Results.Ok(new { data = await handler.HandleAsync(cancellationToken), metadata = Metadata(http) });

    private static async Task<IResult> AreasAsync(
        GetAreasHandler handler,
        HttpContext http,
        CancellationToken cancellationToken) =>
        Results.Ok(new { data = await handler.HandleAsync(cancellationToken), metadata = Metadata(http) });

    private static async Task<IResult> LinesAsync(
        string area,
        GetLinesHandler handler,
        HttpContext http,
        CancellationToken cancellationToken) =>
        Results.Ok(new { data = await handler.HandleAsync(area, cancellationToken), metadata = Metadata(http) });

    private static object Metadata(HttpContext http) => new
    {
        requestId = http.TraceIdentifier,
        servedAtUtc = DateTime.UtcNow,
        apiVersion = "v1",
    };
}
