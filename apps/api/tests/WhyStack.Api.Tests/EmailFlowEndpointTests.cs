using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace WhyStack.Api.Tests;

/// <summary>
/// The confirmation and reset flows, driven the way a person drives them: register, open the mail, take
/// the link out of it, click it.
/// </summary>
public class EmailFlowEndpointTests(WhyStackApiFactory factory) : IClassFixture<WhyStackApiFactory>
{
    private const string Password = "a-good-long-password";
    private const string NewPassword = "a-completely-different-password";

    private static string NewEmail() => $"user-{Guid.CreateVersion7():N}@example.com";

    private static async Task<JsonElement> BodyOf(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

    private async Task<string> RegisterAsync(HttpClient client)
    {
        var email = NewEmail();
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = Password });
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        return email;
    }

    private string LinkTokenFor(string email)
    {
        var mail = factory.Emails.LastTo(email);
        Assert.NotNull(mail);
        return factory.Emails.TokenFrom(mail);
    }

    // ── Confirmation ──────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Registering_then_clicking_the_link_confirms_the_address()
    {
        var client = factory.CreateClient();
        var email = await RegisterAsync(client);

        var confirmed = await client.PostAsJsonAsync(
            "/api/v1/auth/confirm-email",
            new { token = LinkTokenFor(email) });

        Assert.Equal(HttpStatusCode.OK, confirmed.StatusCode);

        // And the account now says so where the client can see it.
        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = Password, platform = "Native" });

        var body = await BodyOf(login);
        Assert.True(body.GetProperty("user").GetProperty("isEmailConfirmed").GetBoolean());
    }

    [Fact]
    public async Task A_confirmation_link_cannot_be_used_twice()
    {
        var client = factory.CreateClient();
        var email = await RegisterAsync(client);
        var token = LinkTokenFor(email);

        await client.PostAsJsonAsync("/api/v1/auth/confirm-email", new { token });
        var second = await client.PostAsJsonAsync("/api/v1/auth/confirm-email", new { token });

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        Assert.Equal("invalid_confirmation_token", (await BodyOf(second)).GetProperty("code").GetString());
    }

    // ── Password reset ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Forgotten_password_can_be_reset_from_the_emailed_link()
    {
        var client = factory.CreateClient();
        var email = await RegisterAsync(client);

        var requested = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email });
        Assert.Equal(HttpStatusCode.Accepted, requested.StatusCode);

        var reset = await client.PostAsJsonAsync(
            "/api/v1/auth/reset-password",
            new { token = LinkTokenFor(email), newPassword = NewPassword });

        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);

        var withNew = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = NewPassword, platform = "Native" });

        var withOld = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = Password, platform = "Native" });

        Assert.Equal(HttpStatusCode.OK, withNew.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, withOld.StatusCode);
    }

    /// <summary>
    /// Changing your password is what you DO when you think someone else is in your account. If their
    /// refresh token still works afterwards, the reset achieved nothing except making the victim feel
    /// safe.
    /// </summary>
    [Fact]
    public async Task Resetting_the_password_signs_out_every_existing_session()
    {
        var client = factory.CreateClient();
        var email = await RegisterAsync(client);

        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = Password, platform = "Native" });

        var sessionToken = (await BodyOf(login)).GetProperty("refreshToken").GetString();

        await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email });
        await client.PostAsJsonAsync(
            "/api/v1/auth/reset-password",
            new { token = LinkTokenFor(email), newPassword = NewPassword });

        var stillAlive = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = sessionToken, platform = "Native" });

        Assert.Equal(HttpStatusCode.Unauthorized, stillAlive.StatusCode);
    }

    /// <summary>
    /// The enumeration guard on the wire. An attacker never sees the Result object — they see a status
    /// code and a body, and both must be identical for an address that exists and one that does not.
    /// </summary>
    [Fact]
    public async Task Forgot_password_answers_identically_for_an_address_with_no_account()
    {
        var client = factory.CreateClient();
        var known = await RegisterAsync(client);

        var forKnown = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = known });
        var forUnknown = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = NewEmail() });

        Assert.Equal(forKnown.StatusCode, forUnknown.StatusCode);
        Assert.Equal(
            await forKnown.Content.ReadAsStringAsync(),
            await forUnknown.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task A_reset_link_cannot_be_used_twice()
    {
        var client = factory.CreateClient();
        var email = await RegisterAsync(client);

        await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email });
        var token = LinkTokenFor(email);

        await client.PostAsJsonAsync("/api/v1/auth/reset-password", new { token, newPassword = NewPassword });

        var second = await client.PostAsJsonAsync(
            "/api/v1/auth/reset-password",
            new { token, newPassword = "another-password-entirely" });

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        Assert.Equal("invalid_reset_token", (await BodyOf(second)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task The_reset_email_never_appears_in_the_response()
    {
        var client = factory.CreateClient();
        var email = await RegisterAsync(client);

        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email });
        var raw = await response.Content.ReadAsStringAsync();

        // The token exists only in the inbox. An API that returned it — even "for convenience in
        // development" — would hand the account to anyone who can make an HTTP request.
        var token = LinkTokenFor(email);
        Assert.DoesNotContain(token, raw, StringComparison.Ordinal);
    }
}
