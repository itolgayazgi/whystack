using Microsoft.AspNetCore.Http.HttpResults;
using WhyStack.Application.Common;

namespace WhyStack.Api.Common;

/// <summary>
/// Turns an Application <see cref="Result{T}"/> into the Problem Details shape `08` mandates.
/// </summary>
/// <remarks>
/// It lives here, once, because `08` says custom error shapes are forbidden and error codes are a
/// contract clients may depend on. An endpoint that builds its own error object is how a contract
/// quietly grows a second dialect.
/// </remarks>
public static class ResultMapping
{
    private const string ErrorDocsBase = "https://docs.whystack.dev/errors/";

    public static IResult ToProblem(this Error error, HttpContext context)
    {
        var (status, title) = Describe(error.Code);

        var extensions = new Dictionary<string, object?>
        {
            // `08` requires a stable, machine-readable code alongside the human-readable title.
            ["code"] = error.Code,
            ["traceId"] = context.TraceIdentifier,
        };

        if (error.FieldErrors is { Count: > 0 })
        {
            // Built by hand rather than with TypedResults.ValidationProblem, which hardcodes 400.
            //
            // `08` requires 422 for validation. 400 is reserved for a request that is structurally
            // broken — malformed JSON — and the distinction is not pedantry: "your JSON is corrupt" and
            // "your password is too short" need completely different things from the client, and one
            // status code for both means it cannot tell the user which happened.
            var validation = new HttpValidationProblemDetails(
                error.FieldErrors.ToDictionary(pair => pair.Key, pair => pair.Value))
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = title,
                Detail = error.Message,
                Instance = context.Request.Path,
                Type = ErrorDocsBase + error.Code.Replace('_', '-'),
            };

            foreach (var (key, value) in extensions)
            {
                validation.Extensions[key] = value;
            }

            return TypedResults.Problem(validation);
        }

        return TypedResults.Problem(
            detail: error.Message,
            instance: context.Request.Path,
            statusCode: status,
            title: title,
            type: ErrorDocsBase + error.Code.Replace('_', '-'),
            extensions: extensions);
    }

    private static (int Status, string Title) Describe(string code) => code switch
    {
        ErrorCodes.ValidationFailed => (StatusCodes.Status422UnprocessableEntity, "Validation failed"),

        // 401, and never 404 or 409. The status code is part of the answer, and an answer that differs
        // between "no such account" and "wrong password" is the enumeration oracle the message was
        // carefully worded to avoid.
        ErrorCodes.InvalidCredentials => (StatusCodes.Status401Unauthorized, "Authentication failed"),

        // 401, like a failed sign-in. The session is over and the caller must authenticate again —
        // which is exactly what 401 means. Anything else (a 400, a 403) would tell a client that has
        // just been signed out to retry, and it would retry forever.
        ErrorCodes.InvalidRefreshToken => (StatusCodes.Status401Unauthorized, "Session ended"),

        // 400, not 401. The caller is not failing to authenticate — they are presenting a link that is
        // spent or expired, and the fix is to request a new one, not to sign in. A 401 would send a
        // client that has no session anyway into an authentication loop.
        ErrorCodes.InvalidResetToken => (StatusCodes.Status400BadRequest, "Reset link is no longer valid"),
        ErrorCodes.InvalidConfirmationToken => (StatusCodes.Status400BadRequest, "Confirmation link is no longer valid"),

        ErrorCodes.AuthenticationRequired => (StatusCodes.Status401Unauthorized, "Authentication required"),
        ErrorCodes.AccessDenied => (StatusCodes.Status403Forbidden, "Access denied"),
        ErrorCodes.AccountLocked => (StatusCodes.Status403Forbidden, "Account locked"),
        ErrorCodes.RateLimitExceeded => (StatusCodes.Status429TooManyRequests, "Rate limit exceeded"),
        _ => (StatusCodes.Status400BadRequest, "Request failed"),
    };
}
