using System.Security.Claims;
using WhyStack.Api.Common;
using WhyStack.Application.Content;
using WhyStack.Application.Content.Authoring;
using WhyStack.Application.Content.Validation;
using WhyStack.Domain.Identity;

namespace WhyStack.Api.Endpoints;

// bind -> validate -> authorize -> call use case -> map response. No business logic, no DbContext.

/// <summary>What "İçerik Üret" posts. A full replacement of the draft, as `08` defines PUT.</summary>
public sealed record SaveTopicRequest(
    Guid? Id,
    string? StableKey,
    string? Slug,
    string? LineKey,
    string? ScopeKey,
    string? Category,
    string? Archetype,

    /// <summary>"OOP II / III", or absent for a stop that is not part of a chain (ADR-0027).</summary>
    SequenceCommand? Sequence,

    string? Level,
    int? EstimatedReadingMinutes,
    IReadOnlyList<string>? SupportedVersions,
    IReadOnlyList<TranslationCommand>? Translations,
    IReadOnlyList<BlockCommand>? Blocks,
    IReadOnlyList<SectionCommand>? Sections,
    IReadOnlyList<ImplementationCommand>? Implementations,
    IReadOnlyList<RelationshipCommand>? Relationships,
    string? RowVersion);

public sealed record TransitionRequest(string? Status, string? Note);

/// <summary>Validate without saving. `forReview` applies the completeness rules a draft is exempt from.</summary>
public sealed record ValidateRequest(SaveTopicRequest Topic, bool? ForReview);

public sealed record SaveTermRequest(
    Guid? Id,
    string? Text,
    IReadOnlyList<string>? Aliases,
    IReadOnlyList<string>? ForbiddenTranslations,
    IReadOnlyList<TermExplanationModel>? Explanations);

/// <summary>Create or rename a scope. <c>LineKey</c> is required on create — a scope lives on a line.</summary>
public sealed record SaveScopeRequest(Guid? Id, string? Key, string? Name, string? LineKey);

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
            .WithSummary("Lines, scopes, ecosystems and every existing topic — everything a dropdown needs.")
            .WithDescription(
                "What the form is built from. The topic list includes DRAFTS — relating a new topic to one "
                + "you are still writing is the normal case.");

        authoring.MapGet("/topics", StudioListAsync)
            .WithName("ListTopicsForStudio")
            .WithSummary("Every topic, at every stage — the editor's workbench.")
            .WithDescription(
                "This is the ONE list that shows drafts. The reader's list refuses them, and the two are "
                + "separate methods on purpose: one endpoint with an includeDrafts flag is one forgotten "
                + "argument away from serving every half-written topic to the internet.");

        authoring.MapGet("/topics/{id:guid}", EditableAsync)
            .WithName("GetTopicForEditing")
            .WithSummary("One topic, in full, for editing.")
            .WithDescription(
                "Every language, every implementation, and the graph as stable KEYS rather than resolved "
                + "links — an editor is about to change them. Its `problems` list is what is wrong with it "
                + "right now: a to-do list, not a rejection.");

        authoring.MapPost("/validate", ValidateAsync)
            .WithName("ValidateTopic")
            .WithSummary("Validate a draft without saving it.")
            .WithDescription(
                "The same rules the save runs, so an editor sees a problem WHILE TYPING rather than after. "
                + "This is a courtesy — the gate is the save and the transition. Pass `forReview: true` to "
                + "apply the stricter completeness rules a topic must pass to leave the author's hands.");

        authoring.MapGet("/terms", TermsAsync)
            .WithName("ListTerms")
            .WithSummary("The terminology dictionary.");

        authoring.MapPost("/terms", SaveTermAsync)
            .WithName("SaveTerm")
            .WithSummary("Create or replace an approved term.")
            .WithDescription(
                "The TERM is preserved; only its explanation is translated. `forbiddenTranslations` names the "
                + "mis-translations by name — a translator rarely drops a term outright, it keeps it in the "
                + "heading and paraphrases it for five paragraphs, and only naming the paraphrase catches that.");

        authoring.MapDelete("/terms/{id:guid}", DeleteTermAsync)
            .WithName("DeleteTerm")
            .WithSummary("Remove a term from the dictionary.");

        /*
          /scopes — all three verbs, one path.

          ADR-0027's rename stopped halfway across this stack: GET and POST became "scopes", DELETE stayed
          "subareas", and the TypeScript client landed on the opposite split — it listed and created against
          /subareas and deleted against /scopes. So every one of the three was a 404, and scope management had
          never worked at all.

          Nothing caught it because nothing checked BOTH ends: the C# compiles, the TypeScript compiles, and a
          route string is just a string on either side. ClientRoutesTests is what checks it now.
        */
        authoring.MapGet("/scopes", SubAreasAsync)
            .WithName("ListScopes")
            .WithSummary("The scopes a topic may be tagged with (ADR-0027).")
            .WithDescription(
                "Each carries the LINE it lives on — a scope's key is unique per line, not globally — and a "
                + "topicCount, which is why a delete may be refused.");

        authoring.MapPost("/scopes", SaveScopeAsync)
            .WithName("SaveScope")
            .WithSummary("Create or rename a scope.")
            .WithDescription(
                "The key and the line are set once and never change — tagged topics and roadmap slices "
                + "resolve through them. An edit changes only the display name.");

        authoring.MapDelete("/scopes/{id:guid}", DeleteScopeAsync)
            .WithName("DeleteScope")
            .WithSummary("Remove a scope — refused if any topic still uses it.")
            .WithDescription(
                "Deleting a scope in use would silently untag its topics, so it is refused with a 409 naming "
                + "the count. Retag those topics first.");

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

    private static async Task<IResult> StudioListAsync(
        IContentAuthoringRepository repository,
        HttpContext http,
        CancellationToken cancellationToken) =>
        Results.Ok(new
        {
            data = await repository.StudioListAsync(cancellationToken),
            metadata = Metadata(http),
        });

    private static async Task<IResult> EditableAsync(
        Guid id,
        IContentAuthoringRepository repository,
        ITopicRepository topics,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var topic = await repository.EditableAsync(id, cancellationToken);

        if (topic is null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not found",
                detail: "No such topic.");
        }

        // The problems are computed HERE, by the layer that owns the rules — not by the repository, which
        // would be a second place that knows what "valid" means (ADR-0020, Decision 3).
        var terminology = await topics.TerminologyAsync(cancellationToken);

        var problems = new TopicValidator(terminology).ValidateDraft(new TopicDraft(
            "en",
            [
                .. topic.Sections.Select(section =>
                    new SectionDraft(section.SectionTypeKey, section.LanguageCode, section.Markdown)),
                .. topic.Implementations
                    .SelectMany(implementation => implementation.Sections)
                    .Select(section =>
                        new SectionDraft(section.SectionTypeKey, section.LanguageCode, section.Markdown)),
            ]));

        return Results.Ok(new
        {
            data = topic with { Problems = problems },
            metadata = Metadata(http),
        });
    }

    private static async Task<IResult> ValidateAsync(
        ValidateRequest request,
        ValidateTopicHandler handler,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var command = ToCommand(request.Topic);

        var result = await handler.HandleAsync(command, request.ForReview ?? false, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(new { data = result.Value, metadata = Metadata(http) })
            : result.Error!.ToProblem(http);
    }

    private static async Task<IResult> TermsAsync(
        IContentAuthoringRepository repository,
        HttpContext http,
        CancellationToken cancellationToken) =>
        Results.Ok(new
        {
            data = await repository.TermsAsync(cancellationToken),
            metadata = Metadata(http),
        });

    private static async Task<IResult> SaveTermAsync(
        SaveTermRequest request,
        IContentAuthoringRepository repository,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Validation failed",
                detail: "A term needs its canonical spelling. That exact string is what must survive translation.");
        }

        var id = await repository.SaveTermAsync(
            new SaveTermCommand(
                request.Id,
                request.Text.Trim(),
                request.Aliases ?? [],
                request.ForbiddenTranslations ?? [],
                request.Explanations ?? []),
            cancellationToken);

        return Results.Ok(new { data = new { id }, metadata = Metadata(http) });
    }

    private static async Task<IResult> DeleteTermAsync(
        Guid id,
        IContentAuthoringRepository repository,
        CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteTermAsync(id, cancellationToken);

        // 204 either way. Deleting something that is not there is not an error — the caller wanted it gone,
        // and it is gone. A 404 here would make a retry after a dropped connection look like a failure.
        return deleted ? Results.NoContent() : Results.NoContent();
    }

    private static async Task<IResult> SubAreasAsync(
        IContentAuthoringRepository repository,
        HttpContext http,
        CancellationToken cancellationToken) =>
        Results.Ok(new
        {
            data = await repository.SubAreasAsync(cancellationToken),
            metadata = Metadata(http),
        });

    private static async Task<IResult> SaveScopeAsync(
        SaveScopeRequest request,
        IContentAuthoringRepository repository,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Validation failed",
                detail: "A scope needs a display name.");
        }

        // The key is required on create and IGNORED on edit (the repository does not touch it). A new scope
        // with no key would be a scope nothing could resolve.
        if (request.Id is null && !IsKey(request.Key))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Validation failed",
                detail: "A scope key is lowercase letters, digits and hyphens — for example \"ef-core\". It "
                    + "is set once and never changes.");
        }

        // Required on create, because a scope only means something on a line (ADR-0027): B1's "Eşzamanlılık"
        // and B3's "Transaction & Eşzamanlılık" are two neighbourhoods, and without the line there is no way
        // to say which one this is.
        if (request.Id is null && string.IsNullOrWhiteSpace(request.LineKey))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Validation failed",
                detail: "A scope needs the line it lives on — for example \"b3-data-access\".");
        }

        var id = await repository.SaveScopeAsync(
            new SaveScopeCommand(
                request.Id,
                (request.Key ?? string.Empty).Trim(),
                request.Name.Trim(),
                (request.LineKey ?? string.Empty).Trim()),
            cancellationToken);

        // Guid.Empty is the repository refusing a line it could not find. A 200 carrying an empty id would be
        // the studio believing it saved something that does not exist.
        if (id == Guid.Empty)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Validation failed",
                detail: $"No line \"{request.LineKey}\".");
        }

        return Results.Ok(new { data = new { id }, metadata = Metadata(http) });
    }

    private static async Task<IResult> DeleteScopeAsync(
        Guid id,
        IContentAuthoringRepository repository,
        CancellationToken cancellationToken)
    {
        var outcome = await repository.DeleteScopeAsync(id, cancellationToken);

        if (!outcome.Deleted && outcome.InUseCount > 0)
        {
            // 409, not 204. A theme in use is not gone, and pretending it is would leave the editor thinking
            // they had cleaned up while every tagged topic still pointed at it.
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Theme is in use",
                detail: $"This theme is used by {outcome.InUseCount} "
                    + $"topic{(outcome.InUseCount == 1 ? "" : "s")}. Retag them before deleting it — otherwise "
                    + "they would be silently untagged.");
        }

        // Deleted, or was not there. Either way the caller's intent — "it should be gone" — holds.
        return Results.NoContent();
    }

    private static bool IsKey(string? key) =>
        !string.IsNullOrWhiteSpace(key)
        && key.All(character => char.IsAsciiLetterLower(character) || char.IsAsciiDigit(character) || character == '-');

    /// <summary>The request shape, mapped to the command. One place, so the two endpoints cannot drift.</summary>
    private static SaveTopicCommand ToCommand(SaveTopicRequest request) => new(
        Id: request.Id,
        // NAMED, not positional. This record has sixteen members and grew twice; a positional call silently
        // reorders the day one is inserted, and the compiler only catches it when two neighbours happen to
        // have different types. Naming them makes that impossible.
        StableKey: request.StableKey ?? string.Empty,
        Slug: request.Slug ?? string.Empty,
        LineKey: request.LineKey ?? string.Empty,

        // Trimmed to null: an empty string from a "— no theme —" dropdown selection means "no theme", not a
        // theme whose key is the empty string.
        ScopeKey: string.IsNullOrWhiteSpace(request.ScopeKey) ? null : request.ScopeKey.Trim(),

        Category: request.Category ?? "Concept",

        // Concept when unsaid: a topic that has not chosen its shape is a plain explanation, and that is the
        // most common one. The studio always sends it; a raw caller may not.
        Archetype: request.Archetype ?? "Concept",

        // Null is a stop that is not part of a chain — the ordinary case, and not an omission the API should
        // fill in. There is no sensible default for "which part of what".
        Sequence: request.Sequence,

        Level: request.Level ?? string.Empty,
        EstimatedReadingMinutes: request.EstimatedReadingMinutes ?? 0,
        SupportedVersions: request.SupportedVersions ?? [],
        Translations: request.Translations ?? [],
        Blocks: request.Blocks ?? [],
        Sections: request.Sections ?? [],
        Implementations: request.Implementations ?? [],
        Relationships: request.Relationships ?? [],
        RowVersion: request.RowVersion);

    private static async Task<IResult> SaveAsync(
        SaveTopicRequest request,
        ClaimsPrincipal principal,
        SaveTopicHandler handler,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(ToCommand(request), principal.Id(), cancellationToken);

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
