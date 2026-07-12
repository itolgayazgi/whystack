using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace WhyStack.Api.Tests;

/// <summary>
/// Drives the real HTTP pipeline against the real database. The Application tests already prove the
/// decisions; these prove the CONTRACT — the status codes, the Problem Details shape, and the fact
/// that none of it leaks what the handlers were careful not to say.
/// </summary>
public class AuthEndpointsTests(WhyStackApiFactory factory) : IClassFixture<WhyStackApiFactory>
{
    private const string GoodPassword = "a-good-long-password";

    /// <summary>A fresh address per test. These tests share one database and must not share state.</summary>
    private static string NewEmail() => $"user-{Guid.CreateVersion7():N}@example.com";

    private static async Task<JsonElement> BodyOf(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

    [Fact]
    public async Task Register_then_login_returns_an_access_token()
    {
        var client = factory.CreateClient();
        var email = NewEmail();

        var registered = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = GoodPassword, displayName = "Ada" });

        Assert.Equal(HttpStatusCode.Accepted, registered.StatusCode);

        var loggedIn = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = GoodPassword });

        Assert.Equal(HttpStatusCode.OK, loggedIn.StatusCode);

        var body = await BodyOf(loggedIn);
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("accessToken").GetString()));
        Assert.Equal(email, body.GetProperty("user").GetProperty("email").GetString());

        // Registration grants exactly one role, and it is not Administrator.
        var roles = body.GetProperty("user").GetProperty("roles").EnumerateArray().Select(r => r.GetString()).ToList();
        Assert.Equal(["RegisteredUser"], roles);
    }

    /// <summary>
    /// The enumeration guard, asserted on the wire rather than in a handler. An attacker never sees the
    /// Result object — they see a status code and a body, and BOTH must be identical.
    /// </summary>
    [Fact]
    public async Task Registering_a_taken_address_is_indistinguishable_from_registering_a_free_one()
    {
        var client = factory.CreateClient();
        var email = NewEmail();

        var first = await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = GoodPassword });
        var second = await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = GoodPassword });

        Assert.Equal(first.StatusCode, second.StatusCode);
        Assert.Equal(
            await first.Content.ReadAsStringAsync(),
            await second.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task An_unknown_account_and_a_wrong_password_are_the_same_401()
    {
        var client = factory.CreateClient();
        var email = NewEmail();
        await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = GoodPassword });

        var unknown = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = NewEmail(), password = GoodPassword });

        var wrongPassword = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = "definitely-not-the-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);

        var unknownBody = await BodyOf(unknown);
        var wrongBody = await BodyOf(wrongPassword);

        Assert.Equal("invalid_credentials", unknownBody.GetProperty("code").GetString());
        Assert.Equal("invalid_credentials", wrongBody.GetProperty("code").GetString());
        Assert.Equal(
            unknownBody.GetProperty("detail").GetString(),
            wrongBody.GetProperty("detail").GetString());
    }

    /// <summary>
    /// `08`: validation errors are 422, not 400, and carry a field-level `errors` object.
    /// A client cannot show the user which field is wrong if the API only says "bad request".
    /// </summary>
    [Fact]
    public async Task A_bad_password_is_a_422_with_the_field_named()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email = NewEmail(), password = "short" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var body = await BodyOf(response);
        Assert.Equal("validation_failed", body.GetProperty("code").GetString());
        Assert.True(body.GetProperty("errors").TryGetProperty("password", out _));
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task The_error_body_never_repeats_the_password_back()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email = "not-an-email", password = "hunter2-hunter2" });

        var raw = await response.Content.ReadAsStringAsync();

        // An error message that echoes the credential puts it in every log, every proxy and every
        // browser devtools panel between here and the user.
        Assert.DoesNotContain("hunter2", raw, StringComparison.OrdinalIgnoreCase);
    }
}
