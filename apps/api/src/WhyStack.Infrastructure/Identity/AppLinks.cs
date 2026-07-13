using Microsoft.Extensions.Options;
using WhyStack.Application.Abstractions;

namespace WhyStack.Infrastructure.Identity;

public sealed class AppOptions
{
    public const string SectionName = "App";

    /// <summary>
    /// Where the CLIENT lives, not the API. The link in an email is clicked by a human in a browser,
    /// and it must land on a screen — not on a JSON endpoint.
    /// </summary>
    public required string ClientBaseUrl { get; init; }
}

public sealed class AppLinks(IOptions<AppOptions> options) : IAppLinks
{
    private readonly Uri _base = new(options.Value.ClientBaseUrl.TrimEnd('/') + "/");

    // The token goes in the query string, and that is a compromise worth naming.
    //
    // Query strings end up in browser history, in Referer headers, and in the access logs of anything
    // in between. For a token that is single-use and dies in an hour, that is acceptable — by the time
    // it surfaces in a log it is already spent. It would NOT be acceptable for a long-lived credential,
    // and it is the reason these tokens expire as fast as they do.
    //
    // The alternative — POSTing the token from a form — cannot survive being pasted into a browser from
    // an email, which is what people actually do.

    // ─────────────────────────────────────────────────────────────────────────────────────────────
    //  THESE PATHS ARE NOT DECORATIVE. THEY ARE A CONTRACT WITH THE CLIENT'S ROUTE TREE.
    //
    //  They were wrong, and every confirmation and reset link this service has ever sent was dead —
    //  a 404, on every platform, from the first day. It was found by a human registering on a real
    //  phone, which is the worst way to find anything.
    //
    //  The cause: the screens live in `apps/client/src/app/(auth)/confirm-email.tsx`, and a folder in
    //  PARENTHESES is an Expo Router "route group" — it organises files and DOES NOT APPEAR IN THE
    //  URL. So the route is `/confirm-email`, and this class was building `/auth/confirm-email`.
    //
    //  Why no test caught it: the endpoint tests generate the link, pull the token back OUT of it with
    //  a string split, and post that token to the API. They never visit the URL. The link could have
    //  said anything at all.
    //
    //  ClientRoutes.cs asserts these strings against the client's actual route files, so the next
    //  person to rename a screen breaks a build instead of breaking everybody's email.
    // ─────────────────────────────────────────────────────────────────────────────────────────────

    public const string ConfirmEmailPath = "confirm-email";

    public const string ResetPasswordPath = "reset-password";

    public string ConfirmEmail(string token) =>
        new Uri(_base, $"{ConfirmEmailPath}?token={Uri.EscapeDataString(token)}").ToString();

    public string ResetPassword(string token) =>
        new Uri(_base, $"{ResetPasswordPath}?token={Uri.EscapeDataString(token)}").ToString();
}
