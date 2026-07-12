using Microsoft.EntityFrameworkCore;
using WhyStack.Api.Common;
using WhyStack.Application.Abstractions;
using WhyStack.Application.Common;
using WhyStack.Application.Identity.Confirmation;
using WhyStack.Application.Identity.Login;
using WhyStack.Application.Identity.Logout;
using WhyStack.Application.Identity.Passwords;
using WhyStack.Application.Identity.Refresh;
using WhyStack.Application.Identity.Register;
using WhyStack.Application.Identity.Sessions;

namespace WhyStack.Api.Endpoints;

// bind -> validate -> call use case -> map response. No business logic, no DbContext (CLAUDE.md §3).
// Everything that decides anything lives in the Application layer; this file only speaks HTTP.

/// <summary>
/// Which half of ADR-0008's token strategy applies to this caller.
/// </summary>
/// <remarks>
/// The API cannot guess. A browser must get the refresh token in an HttpOnly cookie and NOT in the
/// body — otherwise JavaScript can read it and the cookie is decoration. A native client cannot use a
/// cookie at all and must get it in the body, to put in the Keychain or Keystore.
///
/// Handing out both would give the worst of each: the web client would have a JS-readable copy of the
/// very token the cookie exists to hide.
/// </remarks>
public enum ClientPlatform
{
    Web = 0,
    Native = 1,
}

/// <summary>What a client sends to create an account.</summary>
/// <param name="DeviceLocale">
/// The device's locale ("tr-TR", "en-GB", "de-DE"), which picks the starting application language:
/// Turkish device → Turkish, everything else → English (`04`). Optional; absent means English.
///
/// Taken from the body, not from the Accept-Language header. The header would work in a browser and
/// tells you nothing useful from a native app, so relying on it would give the two clients different
/// behaviour for the same person — and the one place that behaviour is visible is the moment they first
/// open the app. The client knows its own locale exactly; it says so.
///
/// It is only a STARTING value. Whatever the user chooses afterwards is the only thing that matters,
/// and GET /users/me/preferences always reports what was actually stored.
/// </param>
public sealed record RegisterRequest(
    string? Email,
    string? Password,
    string? DisplayName,
    string? DeviceLocale);

public sealed record LoginRequest(string? Email, string? Password, ClientPlatform Platform = ClientPlatform.Web);

public sealed record RefreshRequest(string? RefreshToken, ClientPlatform Platform = ClientPlatform.Web);

public sealed record LogoutRequest(string? RefreshToken, bool AllDevices = false);

public sealed record ConfirmEmailRequest(string? Token);

public sealed record ResendConfirmationRequest(string? Email);

public sealed record ForgotPasswordRequest(string? Email);

public sealed record ResetPasswordRequest(string? Token, string? NewPassword);

public sealed record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    /// <summary>Populated for native clients only. On web it is null — the token is in an HttpOnly cookie.</summary>
    string? RefreshToken,
    DateTime? RefreshTokenExpiresAtUtc,
    UserResponse? User);

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
            .WithSummary("Sign in. Returns a short-lived access token and starts a refresh session.");

        auth.MapPost("/refresh", RefreshAsync)
            .WithName("Refresh")
            .WithSummary("Exchange a refresh token for a new access token, and rotate the refresh token.")
            .WithDescription(
                "The refresh token is single-use. Presenting one that has already been rotated revokes "
                + "the entire session family — it means the token leaked.");

        auth.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .WithSummary("End the session. Succeeds even if the token was already gone.");

        auth.MapPost("/confirm-email", ConfirmEmailAsync)
            .WithName("ConfirmEmail")
            .WithSummary("Confirm an email address with the token from the confirmation email.");

        auth.MapPost("/resend-confirmation", ResendConfirmationAsync)
            .WithName("ResendConfirmation")
            .WithSummary("Send a new confirmation link.")
            .WithDescription("Answers the same way whether the address exists, is already confirmed, or does not exist.");

        auth.MapPost("/forgot-password", ForgotPasswordAsync)
            .WithName("ForgotPassword")
            .WithSummary("Request a password reset link.")
            .WithDescription(
                "Answers the same way whether or not the address has an account. A different answer "
                + "would let anyone test an address list against this site.");

        auth.MapPost("/reset-password", ResetPasswordAsync)
            .WithName("ResetPassword")
            .WithSummary("Set a new password using the token from the reset email.")
            .WithDescription("Signs out every device. A reset that leaves an attacker's session alive resets nothing.");

        return app;
    }

    private static async Task<IResult> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        ConfirmEmailHandler handler,
        ITokenHasher hasher,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var (ipHash, userAgentHash) = RequestFingerprint.Of(context, hasher);

        var result = await handler.HandleAsync(
            new ConfirmEmailCommand(request.Token, ipHash, userAgentHash),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(new { email = result.Value.Email, isEmailConfirmed = true })
            : result.Error!.ToProblem(context);
    }

    private static async Task<IResult> ResendConfirmationAsync(
        ResendConfirmationRequest request,
        ResendConfirmationHandler handler,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ResendConfirmationCommand(request.Email),
            cancellationToken);

        return result.IsSuccess
            ? Results.Accepted(value: new { message = result.Value.Message })
            : result.Error!.ToProblem(context);
    }

    private static async Task<IResult> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        ForgotPasswordHandler handler,
        ITokenHasher hasher,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var (ipHash, userAgentHash) = RequestFingerprint.Of(context, hasher);

        var result = await handler.HandleAsync(
            new ForgotPasswordCommand(request.Email, ipHash, userAgentHash),
            cancellationToken);

        return result.IsSuccess
            ? Results.Accepted(value: new { message = result.Value.Message })
            : result.Error!.ToProblem(context);
    }

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        ResetPasswordHandler handler,
        ITokenHasher hasher,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var (ipHash, userAgentHash) = RequestFingerprint.Of(context, hasher);

        var result = await handler.HandleAsync(
            new ResetPasswordCommand(request.Token, request.NewPassword, ipHash, userAgentHash),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblem(context);
        }

        // Every session is gone, including the one this browser is holding. Leaving the cookie behind
        // would mean the next refresh sends a dead token and fails, forever, with no explanation.
        RefreshTokenCookie.Clear(context);

        return Results.Ok(new { sessionsEnded = result.Value.SessionsEnded });
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
                new RegisterUserCommand(
                    request.Email,
                    request.Password,
                    request.DisplayName,
                    ipHash,
                    userAgentHash,
                    request.DeviceLocale),
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
        var result = await handler.HandleAsync(
            new LoginCommand(request.Email, request.Password, SessionContextFor(context, hasher, request.Platform)),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblem(context);
        }

        var login = result.Value;

        return Results.Ok(Respond(
            context,
            request.Platform,
            login.AccessToken,
            login.AccessTokenExpiresAtUtc,
            login.RefreshToken,
            login.RefreshTokenExpiresAtUtc,
            new UserResponse(
                login.UserId,
                login.Email,
                login.DisplayName,
                login.IsEmailConfirmed,
                login.Roles)));
    }

    private static async Task<IResult> RefreshAsync(
        RefreshRequest request,
        RefreshHandler handler,
        ITokenHasher hasher,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        // Web sends nothing in the body — the browser attaches the cookie, and it cannot read it. Native
        // sends the token it kept in secure storage. Reading the cookie first means a web client cannot
        // accidentally (or maliciously) supply a different token in the body.
        var token = RefreshTokenCookie.Read(context) ?? request.RefreshToken;

        var result = await handler.HandleAsync(
            new RefreshCommand(token, SessionContextFor(context, hasher, request.Platform)),
            cancellationToken);

        if (!result.IsSuccess)
        {
            // The session is over, whatever the reason. Leaving a dead cookie in the browser means the
            // next refresh sends it again and fails again, forever, and the user never gets a clean
            // sign-in screen.
            RefreshTokenCookie.Clear(context);
            return result.Error!.ToProblem(context);
        }

        var refreshed = result.Value;

        return Results.Ok(Respond(
            context,
            request.Platform,
            refreshed.AccessToken,
            refreshed.AccessTokenExpiresAtUtc,
            refreshed.RefreshToken,
            refreshed.RefreshTokenExpiresAtUtc,
            user: null));
    }

    private static async Task<IResult> LogoutAsync(
        LogoutRequest request,
        LogoutHandler handler,
        ITokenHasher hasher,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var token = RefreshTokenCookie.Read(context) ?? request.RefreshToken;

        var result = await handler.HandleAsync(
            new LogoutCommand(token, request.AllDevices, SessionContextFor(context, hasher, ClientPlatform.Web)),
            cancellationToken);

        // Cleared unconditionally, and before we even look at the result. Logout is the one operation
        // that must never leave the caller worse off than it found them.
        RefreshTokenCookie.Clear(context);

        return Results.Ok(new { sessionsEnded = result.Value.SessionsEnded });
    }

    /// <summary>
    /// Gives the caller exactly one copy of the refresh token: the cookie, or the body. Never both.
    /// </summary>
    private static AuthResponse Respond(
        HttpContext context,
        ClientPlatform platform,
        string accessToken,
        DateTime accessTokenExpiresAtUtc,
        string refreshToken,
        DateTime refreshTokenExpiresAtUtc,
        UserResponse? user)
    {
        if (platform == ClientPlatform.Web)
        {
            RefreshTokenCookie.Set(context, refreshToken, refreshTokenExpiresAtUtc);

            // Null in the body. Putting it here as well would hand JavaScript the token the HttpOnly
            // cookie exists to keep away from it, and every XSS would become a permanent session theft.
            return new AuthResponse(accessToken, accessTokenExpiresAtUtc, null, null, user);
        }

        return new AuthResponse(
            accessToken,
            accessTokenExpiresAtUtc,
            refreshToken,
            refreshTokenExpiresAtUtc,
            user);
    }

    private static SessionContext SessionContextFor(HttpContext context, ITokenHasher hasher, ClientPlatform platform)
    {
        var (ipHash, userAgentHash) = RequestFingerprint.Of(context, hasher);
        return new SessionContext(platform.ToString(), DeviceType: null, ipHash, userAgentHash);
    }

    /// <summary>SQL Server: 2601 duplicate key row, 2627 unique constraint violation.</summary>
    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 };
}
