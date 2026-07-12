namespace WhyStack.Api.Common;

/// <summary>
/// Where the refresh token lives on the web, and why it is not in the response body.
/// </summary>
/// <remarks>
/// ADR-0008: on web the refresh token goes in an <b>HttpOnly, Secure, SameSite</b> cookie; on mobile it
/// goes to the device's secure storage (Keychain / Keystore). The access token is held in memory on
/// both, and never in <c>localStorage</c>.
///
/// <b>The point of HttpOnly is that JavaScript cannot read it.</b> Which means the API must not hand
/// the same token back in the JSON body — doing that would put it right back in reach of the XSS the
/// cookie exists to survive, and the cookie would be decoration. So the caller says which platform it
/// is, and gets exactly one of the two. Never both.
///
/// <b>SameSite=Strict is the CSRF defence</b> (ADR-0008 §6). The API and the client share a
/// registrable domain (<c>whystack.dev</c>), so the cookie is same-site for our own client and is not
/// sent at all from a third-party page — which is what a CSRF attack is. The refresh endpoint changes
/// state, so this matters: without it, any site the user visits could silently rotate their session.
///
/// <b>Path</b> narrows it further: the cookie is not attached to every request to the API, only to the
/// auth endpoints that need it. A token that is not sent cannot be stolen in transit or logged by a
/// proxy that had no business seeing it.
/// </remarks>
public static class RefreshTokenCookie
{
    public const string Name = "whystack_refresh";

    private const string Path = "/api/v1/auth";

    public static void Set(HttpContext context, string token, DateTime expiresAtUtc)
    {
        context.Response.Cookies.Append(Name, token, Options(context, expiresAtUtc));
    }

    /// <summary>
    /// Deleting a cookie means overwriting it with the same attributes and an expiry in the past.
    /// Get one attribute wrong — a different Path, a different SameSite — and the browser treats it as
    /// a different cookie, keeps the old one, and the user is still signed in on a device they just
    /// signed out of.
    /// </summary>
    public static void Clear(HttpContext context)
    {
        context.Response.Cookies.Delete(Name, Options(context, expiresAtUtc: null));
    }

    public static string? Read(HttpContext context) =>
        context.Request.Cookies.TryGetValue(Name, out var token) ? token : null;

    private static CookieOptions Options(HttpContext context, DateTime? expiresAtUtc) => new()
    {
        HttpOnly = true,

        // Off over plain HTTP so a developer on http://localhost is not silently unable to sign in —
        // the browser would refuse to store a Secure cookie and nothing would explain why. It is on
        // everywhere a real deployment runs, because a refresh token crossing the wire in the clear is
        // a refresh token somebody else now has.
        Secure = context.Request.IsHttps,

        SameSite = SameSiteMode.Strict,
        Path = Path,
        Expires = expiresAtUtc,
        IsEssential = true,
    };
}
