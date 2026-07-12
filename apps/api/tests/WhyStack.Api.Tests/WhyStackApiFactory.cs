using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace WhyStack.Api.Tests;

/// <summary>
/// The real application, against the real database, with a signing key made up for the test run.
/// </summary>
/// <remarks>
/// The API refuses to start without <c>Jwt:SigningKey</c> — by design. That is why this class exists,
/// and the fact that the tests broke the moment that guard was added is the guard proving itself: an
/// environment with no signing key must fail to start, and "environment" includes this one.
///
/// The key here is a throwaway. It signs tokens that live inside one test process, on a host nobody
/// can reach, for a database that is recreated. Sharing it costs nothing.
/// </remarks>
public class WhyStackApiFactory : WebApplicationFactory<Program>
{
    public const string TestSigningKey =
        "test-only-signing-key-not-a-secret-0123456789abcdef0123456789abcdef";

    /// <summary>
    /// Overridden by <see cref="RateLimitedApiFactory"/>. Everywhere else it is effectively off.
    ///
    /// Not a weakening of the test suite — a fix for it. Every request in a test process comes from the
    /// same address, so one shared limiter means the rate-limit test starves every other test of its
    /// budget, and they fail for a reason that has nothing to do with what they assert. The limiter is
    /// tested where it is the subject, and stays out of the way where it is not.
    /// </summary>
    protected virtual int AuthPermitLimit => 10_000;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = TestSigningKey,
                ["Jwt:Issuer"] = "https://api.whystack.test",
                ["Jwt:Audience"] = "https://whystack.test",
                ["RateLimiting:Auth:PermitLimit"] = AuthPermitLimit.ToString(),
            });
        });
    }
}

/// <summary>The same application, with the rate limit turned down so it can actually be tripped.</summary>
public sealed class RateLimitedApiFactory : WhyStackApiFactory
{
    public const int PermitLimit = 3;

    protected override int AuthPermitLimit => PermitLimit;
}
