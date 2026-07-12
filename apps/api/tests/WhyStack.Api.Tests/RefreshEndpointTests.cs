using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace WhyStack.Api.Tests;

/// <summary>
/// The wire contract for ADR-0008's token strategy: a browser gets the refresh token in an HttpOnly
/// cookie and NOT in the body; a native client gets it in the body and no cookie.
/// </summary>
public class RefreshEndpointTests(WhyStackApiFactory factory) : IClassFixture<WhyStackApiFactory>
{
    private const string Password = "a-good-long-password";

    private static string NewEmail() => $"user-{Guid.CreateVersion7():N}@example.com";

    private static async Task<JsonElement> BodyOf(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

    /// <summary>A client that follows no redirects and keeps no cookies — we drive the cookie by hand.</summary>
    private HttpClient NewClient() => factory.CreateClient();

    private async Task<string> RegisterAsync(HttpClient client)
    {
        var email = NewEmail();
        await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = Password });
        return email;
    }

    private static string? RefreshCookieFrom(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.FirstOrDefault(cookie => cookie.StartsWith("whystack_refresh=", StringComparison.Ordinal))
            : null;

    private static string ValueOf(string setCookie) =>
        setCookie.Split(';')[0]["whystack_refresh=".Length..];

    // ── Web ───────────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// The point of HttpOnly is that JavaScript cannot read the token. Returning the same token in the
    /// JSON body would put it right back within reach of the XSS the cookie exists to survive — and the
    /// cookie would be decoration.
    /// </summary>
    [Fact]
    public async Task Web_login_puts_the_refresh_token_in_an_HttpOnly_cookie_and_NOT_in_the_body()
    {
        var client = NewClient();
        var email = await RegisterAsync(client);

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = Password, platform = "Web" });

        var cookie = RefreshCookieFrom(response);

        Assert.NotNull(cookie);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/api/v1/auth", cookie, StringComparison.OrdinalIgnoreCase);

        var body = await BodyOf(response);
        Assert.Equal(JsonValueKind.Null, body.GetProperty("refreshToken").ValueKind);
    }

    // ── Native ────────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// A native client cannot use a cookie and must be handed the token to put in the Keychain or
    /// Keystore. Setting a cookie for it as well would be a second copy of a bearer credential that
    /// nobody reads and nobody revokes.
    /// </summary>
    [Fact]
    public async Task Native_login_returns_the_refresh_token_in_the_body_and_sets_no_cookie()
    {
        var client = NewClient();
        var email = await RegisterAsync(client);

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = Password, platform = "Native" });

        Assert.Null(RefreshCookieFrom(response));

        var body = await BodyOf(response);
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("refreshToken").GetString()));
    }

    // ── Rotation and reuse ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Refreshing_returns_a_new_access_token_and_a_different_refresh_token()
    {
        var client = NewClient();
        var email = await RegisterAsync(client);

        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = Password, platform = "Native" });

        var first = (await BodyOf(login)).GetProperty("refreshToken").GetString();

        var refreshed = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = first, platform = "Native" });

        Assert.Equal(HttpStatusCode.OK, refreshed.StatusCode);

        var body = await BodyOf(refreshed);
        Assert.NotEqual(first, body.GetProperty("refreshToken").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("accessToken").GetString()));
    }

    /// <summary>
    /// The attack, end to end, over HTTP.
    ///
    /// The thief uses the stolen token first and gets a new one. The victim's client then presents the
    /// token it still holds — the replay. Both are signed out, including the token the THIEF just
    /// obtained, which is the only outcome that actually locks them out.
    /// </summary>
    [Fact]
    public async Task Replaying_a_rotated_token_kills_the_thiefs_new_token_too()
    {
        var client = NewClient();
        var email = await RegisterAsync(client);

        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = Password, platform = "Native" });

        var stolen = (await BodyOf(login)).GetProperty("refreshToken").GetString();

        // The thief rotates first.
        var thief = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = stolen, platform = "Native" });

        var thiefsToken = (await BodyOf(thief)).GetProperty("refreshToken").GetString();

        // The victim's client refreshes with the token it still has. This is the replay.
        var replay = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = stolen, platform = "Native" });

        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
        Assert.Equal("invalid_refresh_token", (await BodyOf(replay)).GetProperty("code").GetString());

        // And the thief's freshly minted token is dead as well. Without family revocation this would
        // still work, and they would stay signed in while the victim was not.
        var thiefTriesAgain = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = thiefsToken, platform = "Native" });

        Assert.Equal(HttpStatusCode.Unauthorized, thiefTriesAgain.StatusCode);
    }

    // ── Logout ────────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_clears_the_cookie_and_the_token_stops_refreshing()
    {
        var client = NewClient();
        var email = await RegisterAsync(client);

        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = Password, platform = "Web" });

        var token = ValueOf(RefreshCookieFrom(login)!);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout")
        {
            Content = JsonContent.Create(new { allDevices = false }),
        };
        request.Headers.Add("Cookie", $"whystack_refresh={token}");

        var loggedOut = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, loggedOut.StatusCode);

        // Deleting a cookie means overwriting it with an expiry in the past. If the attributes do not
        // match, the browser keeps the old one and the user is still signed in on a device they just
        // signed out of.
        var cleared = RefreshCookieFrom(loggedOut);
        Assert.NotNull(cleared);
        Assert.Contains("expires=thu, 01 jan 1970", cleared, StringComparison.OrdinalIgnoreCase);

        var afterwards = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = token, platform = "Native" });

        Assert.Equal(HttpStatusCode.Unauthorized, afterwards.StatusCode);
    }

    /// <summary>
    /// Logout must never fail. A client told "logout failed" cannot un-know the token, cannot try
    /// harder, and a user reading it reasonably concludes they are still signed in.
    /// </summary>
    [Fact]
    public async Task Logout_succeeds_with_no_token_at_all()
    {
        var client = NewClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/logout", new { allDevices = false });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
