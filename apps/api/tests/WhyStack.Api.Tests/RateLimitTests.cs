using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace WhyStack.Api.Tests;

/// <summary>
/// Its own class, with its own host, and therefore its own rate limiter.
/// </summary>
/// <remarks>
/// It lives apart because the limiter is keyed by IP address, and every request in a test process
/// comes from the same one. Sharing a host with the other auth tests meant this test ate their budget
/// and they failed with a 429 that had nothing to do with what they were asserting.
///
/// Which is the limiter working, not failing — but a suite where one test breaks another is a suite
/// nobody trusts, and "just raise the limit" would have tested nothing at all.
/// </remarks>
public class RateLimitTests(RateLimitedApiFactory factory) : IClassFixture<RateLimitedApiFactory>
{
    /// <summary>
    /// `08` requires rate limiting on authentication endpoints, a 429, and a Retry-After header.
    /// Without the header the client has to guess, and a client that guesses retries too soon — which
    /// is how a rate limit turns a bad moment into an outage.
    /// </summary>
    [Fact]
    public async Task Too_many_attempts_from_one_address_are_rejected_with_a_retryable_429()
    {
        var client = factory.CreateClient();
        var attempts = RateLimitedApiFactory.PermitLimit + 3;

        HttpResponseMessage? rejected = null;

        for (var attempt = 1; attempt <= attempts && rejected is null; attempt++)
        {
            var response = await client.PostAsJsonAsync(
                "/api/v1/auth/login",
                new { email = $"user-{Guid.CreateVersion7():N}@example.com", password = "a-good-long-password" });

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rejected = response;
            }
        }

        // `?? throw`, not `Assert.NotNull` followed by a dereference.
        //
        // The C# compiler accepts the latter because it trusts xUnit's [NotNull] annotation. CodeQL's
        // dataflow does not model that annotation, and flagged the dereference — correctly, in the sense
        // that the guarantee lived in an attribute rather than in the code. Suppressing the alert would
        // have silenced the tool without changing anything; this makes the value non-nullable at the
        // point of use, and fails with a message that says what was actually expected.
        var limited = rejected
            ?? throw new Xunit.Sdk.XunitException(
                $"The limiter never rejected a request: {attempts} attempts against a permit limit of "
                    + $"{RateLimitedApiFactory.PermitLimit}. Rate limiting is not applied to this endpoint.");

        var body = JsonSerializer.Deserialize<JsonElement>(await limited.Content.ReadAsStringAsync());

        // Problem Details, like every other error. A 429 that returns a bare string is a custom error
        // shape, which `08` forbids — and a client that has to parse two dialects parses neither well.
        Assert.Equal("rate_limit_exceeded", body.GetProperty("code").GetString());
        Assert.Equal(429, body.GetProperty("status").GetInt32());
        Assert.NotNull(limited.Headers.RetryAfter);
    }
}
