using Microsoft.Extensions.Configuration;

namespace WhyStack.Api.Common;

/// <summary>
/// The one place that decides which browser origins may call this API.
/// </summary>
/// <remarks>
/// <b>CORS is a BROWSER mechanism, and only a browser mechanism.</b> The mobile app is not a browser: React
/// Native goes through OkHttp and NSURLSession, which send no <c>Origin</c> header, make no preflight, and
/// have never heard of the same-origin policy. A native client reaches this API whether or not it appears
/// below. <b>So nothing about the phone belongs in this list</b> — and a setup script that thought otherwise
/// is what produced the outage this class exists to prevent.
///
/// <b>The bug.</b> .NET configuration providers do not REPLACE an array — they merge it BY INDEX. User
/// secrets sit above appsettings.json, so writing <c>Cors:AllowedOrigins:0</c> silently overwrites element 0
/// of the shipped array while leaving element 1 alone. The result looks like a merge and behaves like a
/// partial overwrite. <c>dev-mobile.ps1</c> wrote index 0 for Metro; index 0 was the WEBSITE; the website's
/// origin vanished; the browser stopped receiving <c>Access-Control-Allow-Origin</c>; and the sign-in page
/// reported "cannot reach the server" — true, and pointing nowhere near the cause. There is no way to
/// express "append" against a configuration array, and no way to see the collision by reading either file.
///
/// <b>The fix is a shape, not a value.</b> The shipped origins stay an array in appsettings.json.
/// Machine-local extras go in <c>Cors:AdditionalOrigins</c>, a single delimited STRING — and a scalar cannot
/// be index-merged. A provider replaces it whole or does not touch it. The collision is now impossible to
/// write, rather than merely fixed once.
///
/// Malformed entries throw at STARTUP. A trailing slash is the classic one: <c>https://whystack.dev/</c> is
/// not an origin, never matches, and fails as a CORS rejection in a browser console on a machine nobody is
/// looking at. An API that will not serve anyone is a far better failure than an API that silently will not
/// serve the website.
/// </remarks>
public static class CorsOrigins
{
    public const string ShippedKey = "Cors:AllowedOrigins";

    /// <summary>Machine-local origins. A scalar, so no provider can partially overwrite the array above.</summary>
    public const string LocalKey = "Cors:AdditionalOrigins";

    private static readonly char[] Separators = [',', ';'];

    public static string[] Resolve(IConfiguration configuration)
    {
        var shipped = configuration.GetSection(ShippedKey).Get<string[]>() ?? [];

        var local = (configuration[LocalKey] ?? string.Empty)
            .Split(Separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var origins = shipped
            .Concat(local)
            .Select(origin => origin.Trim())
            .Where(origin => origin.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (origins.Length == 0)
        {
            // An API with no origins is not a smaller API — it is one the WEBSITE cannot use. It starts, it
            // reports healthy, it serves the phone flawlessly (a native client needs no origin), and every
            // browser request is blocked before it leaves the machine. The only evidence is a console
            // message on a user's laptop. We have already paid for that lesson once.
            throw new InvalidOperationException(
                "No CORS origins are configured, so no browser can call this API — the website would be "
                + $"unable to sign anybody in. Set '{ShippedKey}' for this environment (as an environment "
                + "variable: Cors__AllowedOrigins__0=https://whystack.dev), or "
                + $"'{LocalKey}' for a machine-local origin.");
        }

        foreach (var origin in origins)
        {
            Validate(origin);
        }

        return origins;
    }

    private static void Validate(string origin)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException(
                $"'{origin}' is not a CORS origin. An origin is scheme + host + port, and nothing else — "
                + "for example http://localhost:3000. Fix it in appsettings, or in the Cors:AdditionalOrigins "
                + "user secret.");
        }

        if (uri.Scheme is not ("http" or "https"))
        {
            throw new InvalidOperationException(
                $"'{origin}' is not an http(s) origin. A browser sends Origin only for http and https.");
        }

        // Everything below is the same mistake wearing different clothes: an origin that carries a path, a
        // query or a fragment never matches ANY request, because a browser sends only scheme+host+port. The
        // trailing slash is the one that catches people — it looks right, and it silently matches nothing.
        if (uri.AbsolutePath != "/" || origin.EndsWith('/') || uri.Query.Length > 0 || uri.Fragment.Length > 0)
        {
            throw new InvalidOperationException(
                $"'{origin}' has a path, a query or a trailing slash. A browser's Origin header is only "
                + $"scheme + host + port, so this would never match anything. Use '{uri.Scheme}://{uri.Authority}'.");
        }
    }
}
