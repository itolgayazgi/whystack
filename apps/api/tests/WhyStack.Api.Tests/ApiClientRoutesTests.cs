using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace WhyStack.Api.Tests;

/// <summary>
/// Every path the TypeScript client calls must be a path this API actually maps.
/// </summary>
/// <remarks>
/// <b>This test exists because three of them were not, and scope management had never worked.</b>
///
/// ADR-0027 renamed SubArea to Scope, and the rename crossed the two ends of the wire. The API moved GET and
/// POST to <c>/scopes</c> and left DELETE on <c>/subareas</c>. The client moved DELETE to <c>/scopes</c> and
/// left GET and POST on <c>/subareas</c>. All three were 404s — listing, creating and deleting a scope — for
/// as long as both files had existed in that state.
///
/// <b>Why nothing caught it.</b> A route is a string on both sides. The C# compiles, the TypeScript compiles,
/// the endpoint tests call the server's own path and pass, and the studio's page tests mock the client away
/// and pass. Every layer was green and the feature was dead. The only thing that could see it is something
/// that reads BOTH files, and until now nothing did.
///
/// <b>What this reads.</b> The routes come from the running application's own EndpointDataSource — not a list
/// somebody maintains here, which would be a third place to forget. The client's paths are parsed out of the
/// api-client source, because that source IS the contract; a mock of it proves only that the mock matches.
/// </remarks>
public class ApiClientRoutesTests(WhyStackApiFactory factory) : IClassFixture<WhyStackApiFactory>
{
    /// <summary>
    /// Every API path the client names, in either shape it writes them.
    /// </summary>
    /// <remarks>
    /// Two shapes, because the client has two: <c>authoring.ts</c> builds on a <c>BASE</c> const, everything
    /// else writes <c>`/api/v1/topics/${slug}`</c> inline. The first version of this only understood BASE and
    /// found nine paths — every one of them from one file, with topics, roadmap, progress and auth unchecked.
    /// The count guard below is what caught that; without it this would have gone green having verified a
    /// tenth of the surface.
    /// </remarks>
    private static readonly Regex ApiPath = new(@"['`](/api/[^'`]*)['`]", RegexOptions.Compiled);

    private static readonly Regex BasePath = new(@"\$\{BASE\}(/[^`]*)`", RegexOptions.Compiled);

    /// <summary>
    /// An interpolation that FOLLOWS A SLASH is a path segment — the same role a route parameter plays.
    /// </summary>
    /// <remarks>
    /// The slash is what makes it a segment. <c>`/topics/${slug}`</c> is a parameter; <c>`/home${suffix}`</c>
    /// is a path with something glued to its end, and what gets glued there is a query string
    /// (<c>?ecosystem=dotnet</c>) built by the caller. Treating both as segments reported <c>/api/v1/home</c>
    /// as a 404 the API serves perfectly well — a false alarm, which is how a test like this gets ignored and
    /// then deleted.
    /// </remarks>
    private static readonly Regex SegmentInterpolation = new(@"(?<=/)\$\{[^}]*\}", RegexOptions.Compiled);

    /// <summary>Anything else glued onto a segment: a query string the caller assembled. Not part of the path.</summary>
    private static readonly Regex TrailingInterpolation = new(@"\$\{[^}]*\}", RegexOptions.Compiled);

    /// <summary>Line and block comments. Prose mentions paths — `/api/v1/auth/*` — and prose is not a call.</summary>
    private static readonly Regex Comments = new(@"//[^\n]*|/\*.*?\*/", RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>A mapped parameter: `{id:guid}`, `{slug}`. Compared by POSITION, not by name.</summary>
    private static readonly Regex RouteParameter = new(@"\{[^}]*\}", RegexOptions.Compiled);

    [Fact]
    public void Every_path_the_client_calls_is_a_path_the_api_maps()
    {
        var mapped = MappedRoutes();
        var called = ClientPaths();

        /*
          A guard on the READER, not on the API.

          This test parses TypeScript. If the shape it parses changes — the client is reformatted, BASE is
          renamed, a module moves — it would find nothing to check and go green having checked nothing, which
          is the exact failure it exists to prevent.

          It has already earned its keep: the first version only understood `${BASE}` and found nine paths, all
          from authoring.ts, leaving topics, roadmap, progress and auth unverified. This line is what said so.

          The floor is deliberately well below the real count (20+) and well above one module's worth.
        */
        Assert.True(
            called.Count >= 18,
            $"Only {called.Count} client paths were found under {ClientSource()}, which is fewer than this "
            + "client has. The parser has stopped seeing some of them, so this test is checking a fraction of "
            + $"the surface and reporting success.\n\nFound:\n  {string.Join("\n  ", called.OrderBy(p => p))}");

        var missing = called.Where(path => !mapped.Contains(path)).OrderBy(path => path).ToList();

        // THE ASSERTION THIS FILE EXISTS FOR.
        Assert.True(
            missing.Count == 0,
            "The client calls paths this API does not map — every one of these is a 404 in production:\n  "
            + string.Join("\n  ", missing)
            + "\n\nThe API maps:\n  "
            + string.Join("\n  ", mapped.OrderBy(path => path)));
    }

    /// <summary>The routes the real application serves, from its own endpoint table.</summary>
    private IReadOnlyCollection<string> MappedRoutes()
    {
        using var scope = factory.Services.CreateScope();

        var endpoints = scope.ServiceProvider
            .GetRequiredService<EndpointDataSource>()
            .Endpoints
            .OfType<RouteEndpoint>()
            .Select(endpoint => $"/{endpoint.RoutePattern.RawText?.TrimStart('/')}")
            .Select(Normalise)
            .ToHashSet(StringComparer.Ordinal);

        Assert.NotEmpty(endpoints);

        return endpoints;
    }

    /// <summary>The paths the api-client actually asks for.</summary>
    private static IReadOnlyCollection<string> ClientPaths()
    {
        var source = ClientSource();

        // A path is compared WHOLE, with its module's BASE prefixed back on: `${BASE}/scopes` in authoring.ts
        // means `/api/v1/content/scopes`, and comparing the tail alone would let /content/scopes match
        // /authoring/scopes.
        return Directory
            .EnumerateFiles(source, "*.ts", SearchOption.AllDirectories)
            .Where(file => !file.EndsWith(".test.ts", StringComparison.Ordinal))
            .SelectMany(file =>
            {
                // Comments first. They discuss paths — "the browser attaches the cookie to /api/v1/auth/*" —
                // and a sentence about a path is not a call to one.
                var text = Comments.Replace(File.ReadAllText(file), string.Empty);

                var basePath = Regex.Match(text, @"const BASE = '([^']+)'");

                var based = basePath.Success
                    ? BasePath.Matches(text).Select(match => basePath.Groups[1].Value + match.Groups[1].Value)
                    : [];

                // The BASE declaration is removed before the inline scan: `/api/v1/content` is a prefix, not
                // an endpoint, and reporting it as an unmapped route is a false alarm every single run.
                var inline = ApiPath
                    .Matches(basePath.Success ? text.Replace(basePath.Value, string.Empty) : text)
                    .Select(match => match.Groups[1].Value);

                return inline.Concat(based).Select(Normalise);
            })
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string ClientSource() =>
        Path.Join(RepositoryRoot(), "packages", "api-client", "src");

    /// <summary>
    /// One shape for both sides: parameters become `{}` and a query string is dropped.
    /// </summary>
    /// <remarks>
    /// `{id:guid}` and `${id}` are the same route in different notations, and comparing the notations would
    /// make this test fail on every route that takes an id — which would get it deleted rather than read.
    /// </remarks>
    private static string Normalise(string path)
    {
        var withoutQuery = path.Split('?')[0];

        // Order matters: a segment interpolation becomes `{}`, and only THEN is whatever is left stripped as
        // a query the caller glued on. Reversed, `/topics/${slug}` would lose its parameter entirely.
        var segments = SegmentInterpolation.Replace(withoutQuery, "{}");
        var withoutTrailing = TrailingInterpolation.Replace(segments, string.Empty);

        return RouteParameter.Replace(withoutTrailing, "{}").TrimEnd('/');
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Join(directory.FullName, "pnpm-workspace.yaml")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Could not find the repository root walking up from {AppContext.BaseDirectory}.");
    }
}
