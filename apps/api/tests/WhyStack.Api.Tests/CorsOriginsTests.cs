using Microsoft.Extensions.Configuration;
using WhyStack.Api.Common;

namespace WhyStack.Api.Tests;

/// <summary>
/// The CORS origin list, and the configuration trap that silently emptied it.
/// </summary>
public class CorsOriginsTests
{
    /// <summary>Two providers, layered the way the real API layers appsettings.json and user secrets.</summary>
    private static IConfiguration Configuration(
        Dictionary<string, string?> appsettings,
        Dictionary<string, string?>? userSecrets = null)
    {
        var builder = new ConfigurationBuilder().AddInMemoryCollection(appsettings);

        if (userSecrets is not null)
        {
            builder.AddInMemoryCollection(userSecrets);
        }

        return builder.Build();
    }

    [Fact]
    public void Ships_the_origins_from_appsettings()
    {
        var origins = CorsOrigins.Resolve(Configuration(new()
        {
            ["Cors:AllowedOrigins:0"] = "http://localhost:3000",
        }));

        Assert.Equal(["http://localhost:3000"], origins);
    }

    /// <summary>
    /// THE BUG, PRESERVED. This is not a test of our code — it is a test of .NET's, and it is here because
    /// nobody believes it until they see it.
    /// </summary>
    /// <remarks>
    /// A higher-priority provider does not REPLACE an array. It merges it BY INDEX. So a user secret that
    /// sets <c>Cors:AllowedOrigins:0</c> — meaning to ADD an origin — deletes whatever appsettings.json had
    /// at index 0 and leaves index 1 standing. The result reads like a merge and behaves like a partial
    /// overwrite, and there is no way to see it by reading either file.
    ///
    /// That is exactly what happened: the setup script wrote Metro at index 0, the website WAS index 0, and
    /// the website's origin ceased to exist. If this assertion ever starts failing, .NET has changed its
    /// merge semantics and <see cref="CorsOrigins.LocalKey"/> can go back to being an array.
    /// </remarks>
    [Fact]
    public void A_second_provider_writing_index_zero_DELETES_the_shipped_origin()
    {
        var origins = CorsOrigins.Resolve(Configuration(
            appsettings: new()
            {
                ["Cors:AllowedOrigins:0"] = "http://localhost:3000",   // the website
                ["Cors:AllowedOrigins:1"] = "http://localhost:4000",
            },
            userSecrets: new()
            {
                ["Cors:AllowedOrigins:0"] = "http://192.168.1.101:8081",
            }));

        // The website is GONE. Not appended to, not merged with — overwritten, in place, silently.
        Assert.DoesNotContain("http://localhost:3000", origins);
        Assert.Contains("http://192.168.1.101:8081", origins);
        Assert.Contains("http://localhost:4000", origins);
    }

    /// <summary>The fix: a scalar has no indices, so there is nothing to collide with.</summary>
    [Fact]
    public void A_local_origin_is_ADDED_and_cannot_erase_a_shipped_one()
    {
        var origins = CorsOrigins.Resolve(Configuration(
            appsettings: new()
            {
                ["Cors:AllowedOrigins:0"] = "http://localhost:3000",
            },
            userSecrets: new()
            {
                ["Cors:AdditionalOrigins"] = "http://192.168.1.101:3000",
            }));

        Assert.Equal(["http://localhost:3000", "http://192.168.1.101:3000"], origins);
    }

    [Fact]
    public void Several_local_origins_may_be_listed()
    {
        var origins = CorsOrigins.Resolve(Configuration(
            appsettings: new() { ["Cors:AllowedOrigins:0"] = "http://localhost:3000" },
            userSecrets: new()
            {
                ["Cors:AdditionalOrigins"] = "http://192.168.1.101:3000, http://10.0.0.7:3000",
            }));

        Assert.Equal(
            ["http://localhost:3000", "http://192.168.1.101:3000", "http://10.0.0.7:3000"],
            origins);
    }

    [Fact]
    public void An_empty_local_key_adds_nothing()
    {
        var origins = CorsOrigins.Resolve(Configuration(new()
        {
            ["Cors:AllowedOrigins:0"] = "http://localhost:3000",
            ["Cors:AdditionalOrigins"] = "",
        }));

        Assert.Equal(["http://localhost:3000"], origins);
    }

    [Fact]
    public void The_same_origin_twice_is_listed_once()
    {
        var origins = CorsOrigins.Resolve(Configuration(
            appsettings: new() { ["Cors:AllowedOrigins:0"] = "http://localhost:3000" },
            userSecrets: new() { ["Cors:AdditionalOrigins"] = "http://localhost:3000" }));

        Assert.Equal(["http://localhost:3000"], origins);
    }

    /// <summary>
    /// A trailing slash is not a typo — it is an origin that matches nothing, forever, in silence.
    /// </summary>
    /// <remarks>
    /// A browser's Origin header is scheme + host + port and nothing else. "https://whystack.dev/" therefore
    /// never equals any Origin a browser will ever send. The API starts, looks healthy, serves the phone
    /// perfectly — and every request from the website is blocked, with the only evidence in a browser console
    /// on somebody else's laptop. Refusing to start is enormously the better failure.
    /// </remarks>
    [Theory]
    [InlineData("http://localhost:3000/")]
    [InlineData("https://whystack.dev/")]
    [InlineData("https://whystack.dev/api")]
    [InlineData("https://whystack.dev?x=1")]
    [InlineData("localhost:3000")]
    [InlineData("not a url")]
    [InlineData("ftp://whystack.dev")]
    public void A_malformed_origin_fails_at_startup(string origin)
    {
        var configuration = Configuration(new() { ["Cors:AllowedOrigins:0"] = origin });

        var error = Assert.Throws<InvalidOperationException>(() => CorsOrigins.Resolve(configuration));

        Assert.Contains(origin, error.Message, StringComparison.Ordinal);
    }

    /// <summary>A machine-local secret is held to exactly the same standard.</summary>
    [Fact]
    public void A_malformed_LOCAL_origin_fails_at_startup_too()
    {
        var configuration = Configuration(
            appsettings: new() { ["Cors:AllowedOrigins:0"] = "http://localhost:3000" },
            userSecrets: new() { ["Cors:AdditionalOrigins"] = "http://192.168.1.101:3000/" });

        Assert.Throws<InvalidOperationException>(() => CorsOrigins.Resolve(configuration));
    }
}
