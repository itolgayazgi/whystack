using Microsoft.EntityFrameworkCore;
using WhyStack.Api.Common;
using WhyStack.Application.Abstractions;
using WhyStack.Application.Common;
using WhyStack.Application.Identity.Login;
using WhyStack.Application.Identity.Register;

namespace WhyStack.Api.Endpoints;

// bind -> validate -> call use case -> map response. No business logic, no DbContext (CLAUDE.md §3).
// Everything that decides anything lives in the Application layer; this file only speaks HTTP.

public sealed record RegisterRequest(string? Email, string? Password, string? DisplayName);

public sealed record LoginRequest(string? Email, string? Password);

public sealed record LoginResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    UserResponse User);

public sealed record UserResponse(
    Guid Id,
    string Email,
    string? DisplayName,
    bool IsEmailConfirmed,
    IReadOnlyCollection<string> Roles);

public static class AuthEndpoints
{
    /// <summary>The rate-limiting policy `08` requires on authentication endpoints.</summary>
    public const string AuthRateLimitPolicy = "auth";

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app
            .MapGroup("/api/v1/auth")
            .WithTags("Authentication")
            .RequireRateLimiting(AuthRateLimitPolicy);

        auth.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Create an account.")
            .WithDescription(
                "Always answers the same way, whether or not the address was already registered. "
                + "That is deliberate: a different answer would let anyone test an address list against "
                + "this site and learn who has an account. The person who owns the inbox is told the truth.");

        auth.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Sign in and receive a short-lived access token.");

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        RegisterUserHandler handler,
        ITokenHasher hasher,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var (ipHash, userAgentHash) = RequestFingerprint.Of(context, hasher);

        try
        {
            var result = await handler.HandleAsync(
                new RegisterUserCommand(request.Email, request.Password, request.DisplayName, ipHash, userAgentHash),
                cancellationToken);

            return result.IsSuccess
                ? Results.Accepted(value: new { message = result.Value.Message })
                : result.Error!.ToProblem(context);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            // The race the handler's comment warns about: two registrations for the same address arrive
            // together, both find nothing, both insert, and the unique index rejects the loser.
            //
            // It is answered with the SAME message as success. Letting the collision surface as a 409
            // would reopen the enumeration oracle through the back door — an attacker who cannot read
            // the reply can still read the status code.
            return Results.Accepted(value: new
            {
                message = "If that address can be registered, we have sent it a confirmation email.",
            });
        }
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        LoginHandler handler,
        ITokenHasher hasher,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var (ipHash, userAgentHash) = RequestFingerprint.Of(context, hasher);

        var result = await handler.HandleAsync(
            new LoginCommand(request.Email, request.Password, ipHash, userAgentHash),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblem(context);
        }

        var login = result.Value;

        // The refresh token is not here yet — it arrives in stage 3, in an HttpOnly cookie on web and
        // secure device storage on mobile (ADR-0008). Until then a session simply ends after fifteen
        // minutes, which is honest: it is better than shipping a long-lived token to make the demo
        // feel finished.
        return Results.Ok(new LoginResponse(
            login.AccessToken,
            login.AccessTokenExpiresAtUtc,
            new UserResponse(
                login.UserId,
                login.Email,
                login.DisplayName,
                login.IsEmailConfirmed,
                login.Roles)));
    }

    /// <summary>SQL Server: 2601 duplicate key row, 2627 unique constraint violation.</summary>
    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 };
}
