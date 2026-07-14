using System.Security.Claims;
using WhyStack.Api.Common;
using WhyStack.Application.Content;
using WhyStack.Application.Content.Authoring;
using WhyStack.Domain.Identity;

namespace WhyStack.Api.Endpoints;

// bind -> validate -> authorize -> call use case -> map response. No business logic, no DbContext.

/// <summary>What "İçerik Üret" posts. A full replacement of the draft, as `08` defines PUT.</summary>
public sealed record SaveTopicRequest(
    Guid? Id,
    string? StableKey,
    string? Slug,
    string? DomainKey,
    string? Category,
    string? Level,
    int? EstimatedReadingMinutes,
    IReadOnlyList<string>? SupportedVersions,
    IReadOnlyList<TranslationCommand>? Translations,
    IReadOnlyList<SectionCommand>? Sections,
    IReadOnlyList<ImplementationCommand>? Implementations,
    IReadOnlyList<RelationshipCommand>? Relationships,
    string? RowVersion);

public sealed record TransitionRequest(string? Status, string? Note);

public static class AuthoringEndpoints
{
    /// <summary>
    /// The roles that may write content.
    /// </summary>
    /// <remarks>
    /// The same three that may READ a draft, and that is not a coincidence: a reviewer who cannot see what
    /// they are approving is a rubber stamp, and an author who cannot see their own draft cannot write.
    ///
    /// RegisteredUser is not on this list, and never will be. Community contribution is Sprint 13's, and `04`
    /// is explicit that it must not be enabled before editorial workflows exist.
    /// </remarks>
    public const string EditorPolicy = "content-editor";

    public static IEndpointRouteBuilder MapAuthoringEndpoints(this IEndpointRouteBuilder app)
    {
        // RequireAuthorization on the GROUP, not on each endpoint. Per-endpoint is opt-IN, and the day
        // somebody adds a route here and forgets the call, an anonymous visitor can write content — and
        // nothing fails, no test breaks, and the hole is invisible until it is found from outside.
        var authoring = app
            .MapGroup("/api/v1/content")
            .WithTags("Authoring")
            .RequireAuthorization(EditorPolicy);

        authoring.MapGet("/catalog", CatalogAsync)
            .WithName("GetAuthoringCatalog")
            .WithSummary("Domains, ecosystems, section types and every existing topic.")
            .WithDescription(
                "What the form is built from. The topic list includes DRAFTS — relating a new topic to one "
                + "you are still writing is the normal case.");

        authoring.MapPost("/topics", SaveAsync)
            .WithName("SaveTopic")
            .WithSummary("Create or replace a topic draft.")
            .WithDescription(
                "A full replacement: what is on screen is what is saved. Content problems come back WITH the "
                + "saved topic — a draft is allowed to have them, and an editor cannot fix what they cannot "
                + "save. rowVersion must be the one from the last read.");

        authoring.MapPost("/topics/{id:guid}/transitions", TransitionAsync)
            .WithName("TransitionTopic")
            .WithSummary("Move a topic through the editorial lifecycle.")
            .WithDescription(
                "One stage at a time. AiDraft → Published is refused: every gate between a draft and a reader "
                + "is a gate a human opens, one at a time. Advancing past AiDraft also requires the topic to "
                + "be complete — mandatory sections, finished translations, terminology preserved.");

        return app;
    }

    private static async Task<IResult> CatalogAsync(
        ITopicRepository repository,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var catalog = await repository.CatalogAsync(cancellationToken);

        return Results.Ok(new { data = catalog, metadata = Metadata(http) });
    }

    private static async Task<IResult> SaveAsync(
        SaveTopicRequest request,
        ClaimsPrincipal principal,
        SaveTopicHandler handler,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var editorId = principal.Id();

        var command = new SaveTopicCommand(
            request.Id,
            request.StableKey ?? string.Empty,
            request.Slug ?? string.Empty,
            request.DomainKey ?? string.Empty,
            request.Category ?? "Concept",
            request.Level ?? string.Empty,
            request.EstimatedReadingMinutes ?? 0,
            request.SupportedVersions ?? [],
            request.Translations ?? [],
            request.Sections ?? [],
            request.Implementations ?? [],
            request.Relationships ?? [],
            request.RowVersion);

        var result = await handler.HandleAsync(command, editorId, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(new { data = result.Value, metadata = Metadata(http) })
            : result.Error!.ToProblem(http);
    }

    private static async Task<IResult> TransitionAsync(
        Guid id,
        TransitionRequest request,
        ClaimsPrincipal principal,
        TransitionTopicHandler handler,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var reviewerId = principal.Id();

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return Results.Problem(statusCode: 422, detail: "status is required.");
        }

        var result = await handler.HandleAsync(id, request.Status, request.Note, reviewerId, cancellationToken);

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

/// <summary>The roles allowed to author and review content (ADR-0005).</summary>
public static class EditorRoles
{
    public static readonly string[] All =
    [
        nameof(RoleName.Editor),
        nameof(RoleName.Reviewer),
        nameof(RoleName.Administrator),
    ];
}
