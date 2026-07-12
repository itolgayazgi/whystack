using System.Text.Json;
using WhyStack.Application.Users;

namespace WhyStack.Application.Tests.Users;

/// <summary>
/// The design token and the C# constant must be the same numbers. This is what makes that true.
/// </summary>
/// <remarks>
/// <b>Without this test, <see cref="ReadingFontScale"/> is a hardcoded design value</b> — precisely
/// what CLAUDE.md §1.8 forbids. The canonical list lives in the theme package, because the client needs
/// it and TypeScript and C# cannot share a module; the API compiles its own copy so that request
/// validation does not depend on reading a frontend package off disk at runtime.
///
/// A copy is only safe if it cannot silently diverge. So: change the token and forget the API, and the
/// build goes red here, naming both files. That is the entire justification for the duplication.
/// </remarks>
public class ReadingFontScaleTokenTests
{
    private sealed record Token(double[] Steps, double Default);

    [Fact]
    public void The_API_and_the_design_token_agree_on_the_steps()
    {
        var token = LoadToken();

        Assert.Equal(token.Steps, ReadingFontScale.Steps);

        // Argument order reads backwards on purpose: xUnit's analyzer requires the constant in the
        // 'expected' slot. The JSON is still the source of truth — the C# constant is the copy, and this
        // assertion is what stops it drifting.
        Assert.Equal(ReadingFontScale.Default, token.Default);
    }

    [Fact]
    public void The_default_is_one_of_the_steps()
    {
        // Obvious, and worth asserting anyway: a default outside the allowed set means every new user is
        // created holding a value their own settings screen would refuse to accept back.
        Assert.Contains(ReadingFontScale.Default, ReadingFontScale.Steps);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(1.5)]
    [InlineData(0.875)]
    public void Allows_a_step(double value) => Assert.True(ReadingFontScale.IsAllowed(value));

    [Theory]
    [InlineData(1.1)]      // between two steps — plausible, and still refused rather than snapped
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(500.0)]    // the reason a range exists at all
    [InlineData(double.NaN)]
    public void Refuses_anything_else(double value) => Assert.False(ReadingFontScale.IsAllowed(value));

    private static Token LoadToken()
    {
        // Path.Join, not Path.Combine. Combine SILENTLY DISCARDS everything before any segment that
        // looks rooted — Path.Combine("a", "/b") is "/b", not "a/b" — which turns a bad segment into a
        // path that points somewhere else entirely rather than into an error. Join concatenates, always.
        var path = Path.Join(RepositoryRoot(), "packages", "theme", "src", "reading-font-scale.json");

        Assert.True(
            File.Exists(path),
            $"The design token is missing: {path}. It is the source of truth for the reading font "
            + "scale — the C# constant is a copy of it, and without the file the copy is unguarded.");

        return JsonSerializer.Deserialize<Token>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    /// <summary>
    /// Walks up from the test binary until it finds the workspace root.
    ///
    /// Not a relative path with a fixed number of "../" segments: that encodes the build's output depth
    /// (bin/Debug/net10.0/...) into a test, and it breaks the first time somebody changes a target
    /// framework or runs the suite from a different working directory. The marker file is what the root
    /// actually IS.
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
            "Could not find the repository root (looked for pnpm-workspace.yaml walking up from "
            + AppContext.BaseDirectory + ").");
    }
}
