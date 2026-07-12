using System.Security.Claims;
using WhyStack.Api.Common;
using WhyStack.Application.Users.Preferences;
using WhyStack.Application.Users.Profile;
using WhyStack.Domain.Users;

namespace WhyStack.Api.Endpoints;

// bind -> authorize -> call use case -> map response. No business logic, no DbContext (CLAUDE.md §3).

/// <summary>
/// A full replacement, as `08` defines <c>PUT</c>. Every field is required, including
/// <see cref="RowVersion"/> — read before you write.
/// </summary>
public sealed record UpdatePreferencesRequest(
    string? ApplicationLanguage,
    string? ContentLanguage,
    ThemeMode? ThemeMode,
    double? ReadingFontScale,
    bool? ReducedMotionEnabled,
    SkillLevel? PreferredSkillLevel,
    string? RowVersion);

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // RequireAuthorization on the GROUP, not on each endpoint.
        //
        // Deliberate. Per-endpoint authorization is opt-IN, and the day someone adds a route to this
        // file and forgets the call, the endpoint is public and nothing fails — no test breaks, no
        // build warns, and the hole is invisible until it is found from outside. On the group it is
        // opt-OUT: a new route is protected by default, and making one public takes a visible,
        // reviewable AllowAnonymous.
        var users = app
            .MapGroup("/api/v1/users")
            .WithTags("Users")
            .RequireAuthorization();

        users.MapGet("/me", GetMeAsync)
            .WithName("GetCurrentUser")
            .WithSummary("The signed-in user's profile.");

        users.MapGet("/me/preferences", GetPreferencesAsync)
            .WithName("GetPreferences")
            .WithSummary("The signed-in user's preferences.");

        users.MapPut("/me/preferences", UpdatePreferencesAsync)
            .WithName("UpdatePreferences")
            .WithSummary("Replace the signed-in user's preferences.")
            .WithDescription(
                "A full replacement: every field is required. rowVersion must be the one from the last "
                + "read — if the preferences changed in between (another device), this returns 409 "
                + "concurrency_conflict rather than silently overwriting that change.");

        return app;
    }

    // Every one of these takes the user id from the TOKEN (ClaimsPrincipal), never from the request.
    // There is no route parameter to tamper with, because there is no route parameter.
    private static async Task<IResult> GetMeAsync(
        ClaimsPrincipal principal,
        GetCurrentUserHandler handler,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(principal.Id(), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.Error!.ToProblem(context);
    }

    private static async Task<IResult> GetPreferencesAsync(
        ClaimsPrincipal principal,
        GetPreferencesHandler handler,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(principal.Id(), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.Error!.ToProblem(context);
    }

    private static async Task<IResult> UpdatePreferencesAsync(
        UpdatePreferencesRequest request,
        ClaimsPrincipal principal,
        UpdatePreferencesHandler handler,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new UpdatePreferencesCommand(
                principal.Id(),
                request.ApplicationLanguage,
                request.ContentLanguage,
                request.ThemeMode,
                request.ReadingFontScale,
                request.ReducedMotionEnabled,
                request.PreferredSkillLevel,
                request.RowVersion),
            cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.Error!.ToProblem(context);
    }
}
