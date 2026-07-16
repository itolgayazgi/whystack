using System.Security.Claims;
using WhyStack.Api.Common;
using WhyStack.Application.Progress;

namespace WhyStack.Api.Endpoints;

// bind -> validate -> authorize -> call use case -> map response. No business logic, no DbContext.

/// <summary>Where the reader is. The server clamps this — see RecordProgressHandler (ADR-0025).</summary>
public sealed record RecordProgressRequest(
    string? Slug,
    string? EcosystemKey,
    int? LastBlockOrder,

    /// <summary>Null leaves completion alone. Only the READER sets it — never inferred from a scroll.</summary>
    bool? Completed);

public static class ProgressEndpoints
{
    public static IEndpointRouteBuilder MapProgressEndpoints(this IEndpointRouteBuilder app)
    {
        // Progress belongs to a person, so every route here needs one. RequireAuthorization on the GROUP:
        // per-endpoint is opt-IN, and the day somebody adds a route and forgets it, one reader's position is
        // written under whichever id the request carried.
        var progress = app
            .MapGroup("/api/v1")
            .WithTags("Progress")
            .RequireAuthorization();

        progress.MapGet("/home", HomeAsync)
            .WithName("GetHome")
            .WithSummary("Everything the home screen shows, in one call.")
            .WithDescription(
                "The streak, where you left off, the basamak chart and what to read next. Four widgets, one "
                + "request — fetching them separately would be four round trips on the screen a reader opens "
                + "first, every time.");

        progress.MapPost("/progress", RecordAsync)
            .WithName("RecordProgress")
            .WithSummary("Record where the reader is, and touch their streak.")
            .WithDescription(
                "The position is CLAMPED to the topic's real block count and never moves backwards — "
                + "scrolling up to re-read the hook must not lose your place. Completion is only ever what "
                + "the reader claims; it is not inferred from reaching the last block.");

        return app;
    }

    private static async Task<IResult> HomeAsync(
        ClaimsPrincipal principal,
        GetHomeHandler handler,
        HttpContext http,
        CancellationToken cancellationToken,
        string? ecosystem = null,
        string? language = null)
    {
        var snapshot = await handler.HandleAsync(
            principal.Id(), ecosystem, language ?? "en", cancellationToken);

        return Results.Ok(new { data = snapshot, metadata = Metadata(http) });
    }

    private static async Task<IResult> RecordAsync(
        RecordProgressRequest request,
        ClaimsPrincipal principal,
        RecordProgressHandler handler,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Slug))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Validation failed",
                detail: "A slug is required — progress is progress in a topic.");
        }

        var result = await handler.HandleAsync(
            principal.Id(),
            request.Slug,
            string.IsNullOrWhiteSpace(request.EcosystemKey) ? null : request.EcosystemKey,
            request.LastBlockOrder ?? 0,
            request.Completed,
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(new { data = result.Value, metadata = Metadata(http) })
            : result.Error!.ToProblem(http);
    }

    private static object Metadata(HttpContext http) => new
    {
        requestId = http.TraceIdentifier,
        servedAtUtc = DateTime.UtcNow,
        apiVersion = "v1",
    };
}
