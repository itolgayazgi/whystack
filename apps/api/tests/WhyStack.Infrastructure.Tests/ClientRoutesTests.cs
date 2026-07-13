using Microsoft.Extensions.Options;
using WhyStack.Infrastructure.Identity;

namespace WhyStack.Infrastructure.Tests;

/// <summary>
/// The paths in an emailed link must be paths the client actually has.
/// </summary>
/// <remarks>
/// <b>This test exists because they were not, and nobody noticed.</b>
///
/// <c>AppLinks</c> built <c>/auth/confirm-email</c>. The screen lives at
/// <c>apps/client/src/app/(auth)/confirm-email.tsx</c> — and a folder in PARENTHESES is an Expo Router
/// <i>route group</i>: it organises files and <b>does not appear in the URL</b>. The real route is
/// <c>/confirm-email</c>.
///
/// So every confirmation and every password-reset link this service had ever sent was a 404. On every
/// platform. From the first day. It was found by a human registering on a real phone.
///
/// <b>Why the existing tests could not catch it.</b> The endpoint tests generate the link, split the
/// token back out of it with <c>Split("token=")</c>, and post that token to the API. They never visit
/// the URL. The path could have said <c>/banana</c> and every one of them would still be green — which
/// is the whole lesson: a test that reads a value back out of the thing it just wrote has tested
/// nothing about the thing.
///
/// This reads the CLIENT'S OWN ROUTE TREE off disk. Rename a screen and this breaks, loudly, instead of
/// everybody's email breaking, silently.
/// </remarks>
public class ClientRoutesTests
{
    [Fact]
    public void The_confirmation_link_points_at_a_screen_that_exists()
    {
        Assert.Contains(AppLinks.ConfirmEmailPath, ClientRoutes());
    }

    [Fact]
    public void The_reset_link_points_at_a_screen_that_exists()
    {
        Assert.Contains(AppLinks.ResetPasswordPath, ClientRoutes());
    }

    /// <summary>
    /// The link the API would really send, end to end, with the real path-building code.
    /// </summary>
    [Fact]
    public void A_confirmation_link_is_a_url_a_browser_can_open()
    {
        var links = new AppLinks(Options.Create(new AppOptions
        {
            ClientBaseUrl = "https://whystack.dev",
        }));

        var link = links.ConfirmEmail("a-token-with/special+chars=");

        Assert.StartsWith("https://whystack.dev/confirm-email?token=", link);

        // The token is escaped. A '+' arriving unescaped in a query string is decoded as a SPACE, and
        // the confirmation then fails with "this link no longer works" for a link that was perfect.
        Assert.DoesNotContain("+chars", link);
        Assert.Contains("%2B", link);
    }

    /// <summary>
    /// Every URL the client can be opened at, derived from the file tree the way Expo Router does it.
    /// </summary>
    private static IReadOnlyCollection<string> ClientRoutes()
    {
        var appDirectory = Path.Join(RepositoryRoot(), "apps", "client", "src", "app");

        Assert.True(
            Directory.Exists(appDirectory),
            $"The client's route tree is missing: {appDirectory}. In Expo Router the file tree IS the "
            + "route tree (`06`), so if this directory has moved, the links in every email have moved "
            + "with it.");

        return Directory
            .EnumerateFiles(appDirectory, "*.tsx", SearchOption.AllDirectories)
            .Select(file => Path.GetRelativePath(appDirectory, file))
            .Select(relative => relative.Replace('\\', '/'))

            // `_layout.tsx` is not a route; it wraps them.
            .Where(route => !route.EndsWith("_layout.tsx", StringComparison.Ordinal))

            .Select(route => route[..^".tsx".Length])

            // THE RULE THAT WAS MISSED. A segment in parentheses is a route GROUP: it exists to organise
            // files and is invisible in the URL. `(auth)/confirm-email.tsx` is served at
            // `/confirm-email`, not at `/auth/confirm-email`.
            .Select(route => string.Join(
                '/',
                route.Split('/').Where(segment => !segment.StartsWith('(') || !segment.EndsWith(')'))))

            // `index.tsx` is the directory itself.
            .Select(route => route.EndsWith("index", StringComparison.Ordinal)
                ? route[..^"index".Length].TrimEnd('/')
                : route)

            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Walks up until it finds the workspace root, rather than counting "../" segments — which would
    /// encode the build's output depth into a test and break on the first target-framework change.
    /// </summary>
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
