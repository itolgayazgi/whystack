using System.Security.Claims;
using WhyStack.Api.Common;
using WhyStack.Application.Content;
using WhyStack.Domain.Identity;

namespace WhyStack.Api.Endpoints;

// bind -> validate -> authorize -> call use case -> map response. No business logic, no DbContext.

public static class TopicsEndpoints
{
    /// <summary>
    /// The roles that may read a topic before a human has approved it.
    /// </summary>
    /// <remarks>
    /// `04`: "Draft content is not publicly accessible." Not PUBLICLY — an editor has to be able to read
    /// what they are about to review, or the review is a form nobody can fill in.
    ///
    /// Everyone else, signed in or not, sees Published only. That is what makes CLAUDE.md §1.5 true at the
    /// boundary rather than merely intended: a model's draft cannot reach a learner, whatever anyone forgets.
    /// </remarks>
    private static readonly string[] MayReadDrafts =
    [
        nameof(RoleName.Editor),
        nameof(RoleName.Reviewer),
        nameof(RoleName.Administrator),
    ];

    public static IEndpointRouteBuilder MapTopicEndpoints(this IEndpointRouteBuilder app)
    {
        // AllowAnonymous, deliberately. Published educational content is public — ADR-0009 builds a whole
        // static site on that premise, and a reader who found a topic through a search engine must be able
        // to read it without an account. The gate here is the topic's STATUS, not the reader's session.
        var topics = app
            .MapGroup("/api/v1/topics")
            .WithTags("Topics")
            .AllowAnonymous();

        topics.MapGet("/", ListAsync)
            .WithName("ListTopics")
            .WithSummary("Published topics, filtered and paged.")
            .WithDescription(
                "Drafts are included only for an Editor, Reviewer or Administrator. The language of every "
                + "row is stated in its `language` object — a row served in English to a Turkish reader "
                + "says so.");

        topics.MapGet("/{slug}", GetAsync)
            .WithName("GetTopic")
            .WithSummary("One topic, in full.")
            .WithDescription(
                "`language` says which language was asked for and which was served. If the requested "
                + "translation does not exist, the canonical text is returned with `fallbackUsed: true` — "
                + "never silently.");

        return app;
    }

    private static async Task<IResult> ListAsync(
        ClaimsPrincipal principal,
        ListTopicsHandler handler,
        HttpContext http,
        CancellationToken cancellationToken,
        string? technology = null,
        string? level = null,
        string? language = null,
        int? pageNumber = null,
        int? pageSize = null)
    {
        var result = await handler.HandleAsync(
            technology,
            level,
            LanguageOf(language),
            pageNumber,
            pageSize,
            MayReadDraftsAs(principal),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(new
            {
                data = result.Value.Items,
                pagination = new
                {
                    pageNumber = result.Value.PageNumber,
                    pageSize = result.Value.PageSize,
                    totalCount = result.Value.TotalCount,
                    totalPages = result.Value.TotalPages,
                    hasPreviousPage = result.Value.HasPreviousPage,
                    hasNextPage = result.Value.HasNextPage,
                },
                metadata = Metadata(http),
            })
            : result.Error!.ToProblem(http);
    }

    private static async Task<IResult> GetAsync(
        string slug,
        ClaimsPrincipal principal,
        GetTopicHandler handler,
        HttpContext http,
        CancellationToken cancellationToken,
        string? language = null)
    {
        var result = await handler.HandleAsync(
            slug,
            LanguageOf(language),
            MayReadDraftsAs(principal),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(new { data = result.Value, metadata = Metadata(http) })
            : result.Error!.ToProblem(http);
    }

    /// <summary>
    /// The content language. Defaults to English — the canonical one — when the caller does not say.
    /// </summary>
    /// <remarks>
    /// From the QUERY STRING, not from the user's saved preference, and that is deliberate: a topic URL has
    /// to mean the same thing for everyone who opens it. A shared link that renders differently depending
    /// on who clicks it cannot be cached, cannot be indexed (ADR-0009), and cannot be discussed.
    ///
    /// The client sends the reader's preference as `?language=` — so the preference still decides, and it
    /// decides somewhere a URL can carry.
    /// </remarks>
    private static string LanguageOf(string? language) =>
        string.IsNullOrWhiteSpace(language) ? "en" : language.Trim().ToLowerInvariant();

    private static bool MayReadDraftsAs(ClaimsPrincipal principal) =>
        MayReadDrafts.Any(principal.IsInRole);

    private static object Metadata(HttpContext http) => new
    {
        requestId = http.TraceIdentifier,
        servedAtUtc = DateTime.UtcNow,
        apiVersion = "v1",
    };
}
