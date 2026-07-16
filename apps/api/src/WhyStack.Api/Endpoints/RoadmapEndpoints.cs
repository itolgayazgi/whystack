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

        roadmap.MapGet("/lines", LinesAsync)
            .WithName("GetLines")
            .WithSummary("The ecosystem tabs, including the ones with no content yet.");

        return app;
    }

    private static async Task<IResult> GetAsync(
        ClaimsPrincipal principal,
        GetRoadmapHandler handler,
        HttpContext http,
        CancellationToken cancellationToken,
        string? ecosystem = null,
        string? domain = null,
        string? language = null)
    {
        if (string.IsNullOrWhiteSpace(ecosystem) || string.IsNullOrWhiteSpace(domain))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Validation failed",
                detail: "Both `ecosystem` and `domain` are required — a line is one ecosystem through one domain.");
        }

        var result = await handler.HandleAsync(
            principal.Id(), ecosystem, domain, language ?? "en", cancellationToken);

        return result.IsSuccess
            ? Results.Ok(new { data = result.Value, metadata = Metadata(http) })
            : result.Error!.ToProblem(http);
    }

    private static async Task<IResult> LinesAsync(
        GetLinesHandler handler,
        HttpContext http,
        CancellationToken cancellationToken) =>
        Results.Ok(new { data = await handler.HandleAsync(cancellationToken), metadata = Metadata(http) });

    private static object Metadata(HttpContext http) => new
    {
        requestId = http.TraceIdentifier,
        servedAtUtc = DateTime.UtcNow,
        apiVersion = "v1",
    };
}
