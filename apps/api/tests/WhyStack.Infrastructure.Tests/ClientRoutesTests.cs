using Microsoft.Extensions.Options;
using WhyStack.Infrastructure.Identity;

namespace WhyStack.Infrastructure.Tests;

/// <summary>
/// The paths in an emailed link must be paths the WEBSITE actually has.
/// </summary>
/// <remarks>
/// <b>This test exists because they were not, and nobody noticed.</b>
///
/// <c>AppLinks</c> built <c>/auth/confirm-email</c>. The screen lived in a route group — a folder in
/// PARENTHESES, which organises files and <b>does not appear in the URL</b>. The real route was
/// <c>/confirm-email</c>.
///
/// So every confirmation and every password-reset link this service had ever sent was a 404. On every
/// platform. From the first day. It was found by a human registering on a real phone.
///
/// <b>Why the existing tests could not catch it.</b> The endpoint tests generate the link, split the token
/// back out of it with <c>Split("token=")</c>, and post that token to the API. They never visit the URL.
/// The path could have said <c>/banana</c> and every one of them would still be green — which is the whole
/// lesson: a test that reads a value back out of the thing it just wrote has tested nothing about the thing.
///
/// <b>The target moved (ADR-0022), and so did this test.</b> Emails now land on <c>apps/web</c> — a mail
/// link is clicked in a browser, often on a laptop, and the mobile app has no deep-link handler and is not
/// even installed there. Next.js uses the same route-group convention as Expo Router, so the rule this test
/// enforces is unchanged; only the tree it reads is.
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
    /// Every URL the website can be opened at, derived from the file tree the way Next.js does it.
    /// </summary>
    private static IReadOnlyCollection<string> ClientRoutes()
    {
        var appDirectory = Path.Join(RepositoryRoot(), "apps", "web", "src", "app");

        Assert.True(
            Directory.Exists(appDirectory),
            $"The website's route tree is missing: {appDirectory}. The file tree IS the route tree, so if "
            + "this directory has moved, the links in every email have moved with it.");

        return Directory
            .EnumerateFiles(appDirectory, "page.tsx", SearchOption.AllDirectories)
            .Select(file => Path.GetRelativePath(appDirectory, file))
            .Select(relative => relative.Replace('\\', '/'))

            // `app/(auth)/sign-in/page.tsx` → `(auth)/sign-in`. The file name is not part of the URL.
            //
            // `app/page.tsx` is the ROOT — it has no directory in front of it, so trimming "/page.tsx" from
            // an 8-character string asks for a negative length. The first version of this did exactly that
            // and threw before it could check anything, which is the wrong kind of failure: a test that
            // crashes is a test that proves nothing.
            .Select(route => route.Length > "page.tsx".Length
                ? route[..^"/page.tsx".Length]
                : string.Empty)

            // THE RULE THAT WAS MISSED, and Next.js has it too. A segment in parentheses is a route GROUP:
            // it organises files and is INVISIBLE in the URL. `(auth)/confirm-email/page.tsx` is served at
            // `/confirm-email`, not at `/auth/confirm-email`.
            .Select(route => string.Join(
                '/',
                route.Split('/').Where(segment => !segment.StartsWith('(') || !segment.EndsWith(')'))))

            .Where(route => route.Length > 0)

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
