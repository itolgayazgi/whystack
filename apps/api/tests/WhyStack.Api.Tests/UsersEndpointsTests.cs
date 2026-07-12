using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace WhyStack.Api.Tests;

/// <summary>
/// The authenticated surface, over real HTTP against the real database.
/// </summary>
public class UsersEndpointsTests(WhyStackApiFactory factory) : IClassFixture<WhyStackApiFactory>
{
    private const string GoodPassword = "a-good-long-password";

    private static string NewEmail() => $"user-{Guid.CreateVersion7():N}@example.com";

    private static async Task<JsonElement> BodyOf(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

    /// <summary>Registers, signs in, and returns a client that carries the access token.</summary>
    private async Task<(HttpClient Client, string Email)> SignedIn(string? deviceLocale = null)
    {
        var client = factory.CreateClient();
        var email = NewEmail();

        var registered = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = GoodPassword, displayName = "Ada", deviceLocale });

        Assert.Equal(HttpStatusCode.Accepted, registered.StatusCode);

        var loggedIn = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = GoodPassword });

        var token = (await BodyOf(loggedIn)).GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return (client, email);
    }

    private static async Task<JsonElement> ReadPreferences(HttpClient client)
    {
        var response = await client.GetAsync("/api/v1/users/me/preferences");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return await BodyOf(response);
    }

    /// <summary>
    /// The PUT body, as a named type rather than an anonymous object.
    ///
    /// Anonymous objects cannot hold a bare <c>null</c> — the compiler cannot infer a type for it — so
    /// every test that wanted to omit the skill level had to write <c>(string?)null</c>. A record with
    /// nullable properties says the same thing once, and says it in the type instead of at five call
    /// sites.
    /// </summary>
    private sealed record PreferencesBody
    {
        public string? ApplicationLanguage { get; init; } = "tr";
        public string? ContentLanguage { get; init; } = "en";
        public string? ThemeMode { get; init; } = "Dark";
        public double? ReadingFontScale { get; init; } = 1.25;
        public bool? ReducedMotionEnabled { get; init; } = true;
        public string? PreferredSkillLevel { get; init; } = "MidLevel";
        public string? RowVersion { get; init; }
    }

    /// <summary>A complete, valid body built from a fresh read. Tests then break one field at a time.</summary>
    private static PreferencesBody ValidBody(JsonElement current) =>
        new() { RowVersion = current.GetProperty("rowVersion").GetString() };

    // ---------------------------------------------------------------------------------------------
    // Authorization
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// The group is protected, so a route added to it later is protected by default. This is the test
    /// that would catch someone mapping a new /users route and forgetting to think about it.
    /// </summary>
    [Theory]
    [InlineData("GET", "/api/v1/users/me")]
    [InlineData("GET", "/api/v1/users/me/preferences")]
    [InlineData("PUT", "/api/v1/users/me/preferences")]
    public async Task Every_user_endpoint_refuses_an_anonymous_caller(string method, string path)
    {
        var client = factory.CreateClient();

        using var request = new HttpRequestMessage(new HttpMethod(method), path);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_forged_token_is_refused()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not.a.token");

        var response = await client.GetAsync("/api/v1/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// <b>The ownership boundary.</b> Two accounts exist; the caller holds one token; there is no way to
    /// ask for the other. The id comes from the TOKEN, never from the request — which is why the route
    /// is <c>/users/me</c> and not <c>/users/{id}</c>, and why this test cannot even be written as an
    /// attempt to fetch someone else's row. That is the point: the API gives the caller no vocabulary
    /// for naming another user.
    /// </summary>
    [Fact]
    public async Task A_caller_only_ever_sees_their_own_account()
    {
        var (clientA, emailA) = await SignedIn();
        var (clientB, emailB) = await SignedIn();

        var meA = await BodyOf(await clientA.GetAsync("/api/v1/users/me"));
        var meB = await BodyOf(await clientB.GetAsync("/api/v1/users/me"));

        Assert.Equal(emailA, meA.GetProperty("email").GetString());
        Assert.Equal(emailB, meB.GetProperty("email").GetString());
        Assert.NotEqual(meA.GetProperty("id").GetString(), meB.GetProperty("id").GetString());

        // And B's write does not touch A.
        var currentB = await ReadPreferences(clientB);
        await clientB.PutAsJsonAsync("/api/v1/users/me/preferences", ValidBody(currentB));

        var afterA = await ReadPreferences(clientA);
        Assert.Equal("en", afterA.GetProperty("applicationLanguage").GetString());
    }

    // ---------------------------------------------------------------------------------------------
    // Profile
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public async Task The_profile_carries_what_04_asks_for_and_nothing_more()
    {
        var (client, email) = await SignedIn();

        var me = await BodyOf(await client.GetAsync("/api/v1/users/me"));

        Assert.Equal(email, me.GetProperty("email").GetString());
        Assert.Equal("Ada", me.GetProperty("displayName").GetString());
        Assert.False(me.GetProperty("isEmailConfirmed").GetBoolean());
        Assert.True(me.TryGetProperty("createdAtUtc", out _));
        Assert.Equal(["RegisteredUser"], me.GetProperty("roles").EnumerateArray().Select(r => r.GetString()));

        // Never on the wire. Not once, not "internally", not "only for the owner".
        Assert.False(me.TryGetProperty("passwordHash", out _));
        Assert.False(me.TryGetProperty("normalizedEmail", out _));
    }

    // ---------------------------------------------------------------------------------------------
    // Device language (`04` — Device Language Detection)
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public async Task A_Turkish_device_gets_a_Turkish_account()
    {
        var (client, _) = await SignedIn(deviceLocale: "tr-TR");

        var preferences = await ReadPreferences(client);

        Assert.Equal("tr", preferences.GetProperty("applicationLanguage").GetString());
        Assert.Equal("tr", preferences.GetProperty("contentLanguage").GetString());
    }

    [Theory]
    [InlineData("de-DE")]
    [InlineData("en-GB")]
    [InlineData(null)]
    public async Task Every_other_device_gets_an_English_account(string? locale)
    {
        var (client, _) = await SignedIn(deviceLocale: locale);

        var preferences = await ReadPreferences(client);

        Assert.Equal("en", preferences.GetProperty("applicationLanguage").GetString());
    }

    // ---------------------------------------------------------------------------------------------
    // Preferences contract
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// Enums cross the wire as STRINGS (CLAUDE.md §4). The first version of the auth endpoints shipped
    /// them as numbers and nothing failed — the API just silently demanded `platform: 0`.
    /// </summary>
    [Fact]
    public async Task Enums_are_strings_on_the_wire_in_both_directions()
    {
        var (client, _) = await SignedIn();
        var current = await ReadPreferences(client);

        Assert.Equal(JsonValueKind.String, current.GetProperty("themeMode").ValueKind);
        Assert.Equal("System", current.GetProperty("themeMode").GetString());

        var updated = await client.PutAsJsonAsync("/api/v1/users/me/preferences", ValidBody(current));
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        var body = await BodyOf(updated);
        Assert.Equal("Dark", body.GetProperty("themeMode").GetString());
        Assert.Equal("MidLevel", body.GetProperty("preferredSkillLevel").GetString());
    }

    [Fact]
    public async Task A_saved_preference_is_still_there_on_the_next_read()
    {
        var (client, _) = await SignedIn();
        var current = await ReadPreferences(client);

        await client.PutAsJsonAsync("/api/v1/users/me/preferences", ValidBody(current));

        // A fresh request, a fresh DbContext. Asserting on the PUT's own response would only prove the
        // handler mapped its own in-memory object back out — not that anything reached the database.
        var reread = await ReadPreferences(client);

        Assert.Equal("tr", reread.GetProperty("applicationLanguage").GetString());
        Assert.Equal("en", reread.GetProperty("contentLanguage").GetString());
        Assert.Equal("Dark", reread.GetProperty("themeMode").GetString());
        Assert.Equal(1.25, reread.GetProperty("readingFontScale").GetDouble());
        Assert.True(reread.GetProperty("reducedMotionEnabled").GetBoolean());
    }

    /// <summary>
    /// Two devices, one account, one lost update — prevented, over real HTTP, against a real SQL Server
    /// rowversion. The Application test proves the decision; this proves the DATABASE actually enforces
    /// it, which is the half a fake cannot.
    /// </summary>
    [Fact]
    public async Task A_stale_write_is_rejected_with_409_and_does_not_overwrite_the_other_device()
    {
        var (client, _) = await SignedIn();

        // Both devices load the same preferences.
        var asSeenByPhone = await ReadPreferences(client);
        var asSeenByLaptop = asSeenByPhone;

        // The phone saves. It wins.
        var phone = await client.PutAsJsonAsync("/api/v1/users/me/preferences", ValidBody(asSeenByPhone));
        Assert.Equal(HttpStatusCode.OK, phone.StatusCode);

        // The laptop saves, holding the rowVersion it loaded before the phone wrote.
        var laptop = await client.PutAsJsonAsync(
            "/api/v1/users/me/preferences",
            ValidBody(asSeenByLaptop) with
            {
                ThemeMode = "Light",
                ReadingFontScale = 1.0,
                PreferredSkillLevel = null,
            });

        Assert.Equal(HttpStatusCode.Conflict, laptop.StatusCode);

        var problem = await BodyOf(laptop);
        Assert.Equal("concurrency_conflict", problem.GetProperty("code").GetString());

        // The phone's change survived. This is the assertion that matters: a 409 that still wrote the
        // row would be a lost update with a misleading status code on top.
        var reread = await ReadPreferences(client);
        Assert.Equal("Dark", reread.GetProperty("themeMode").GetString());
    }

    [Fact]
    public async Task A_fresh_rowVersion_lets_the_next_write_through()
    {
        var (client, _) = await SignedIn();

        var first = await client.PutAsJsonAsync(
            "/api/v1/users/me/preferences", ValidBody(await ReadPreferences(client)));
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        // Re-read, then write again. Without this, "the second write is rejected" would be indis-
        // tinguishable from a bug that rejects EVERY second write.
        var second = await client.PutAsJsonAsync(
            "/api/v1/users/me/preferences", ValidBody(await ReadPreferences(client)));

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
    }

    /// <summary>`08`: validation is 422, not 400. 400 is for a request that is structurally broken.</summary>
    [Fact]
    public async Task An_out_of_range_reading_scale_is_422_with_a_field_error()
    {
        var (client, _) = await SignedIn();
        var current = await ReadPreferences(client);

        var response = await client.PutAsJsonAsync(
            "/api/v1/users/me/preferences",
            // 500x — the one that breaks every screen in the product.
            ValidBody(current) with { ReadingFontScale = 500.0 });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var problem = await BodyOf(response);
        Assert.Equal("validation_failed", problem.GetProperty("code").GetString());
        Assert.True(problem.GetProperty("errors").TryGetProperty("readingFontScale", out _));

        // And nothing was stored. Not clamped to 1.5, not snapped to the nearest step — refused.
        var reread = await ReadPreferences(client);
        Assert.Equal(1.0, reread.GetProperty("readingFontScale").GetDouble());
    }

    [Fact]
    public async Task Writing_without_a_rowVersion_is_refused_rather_than_allowed_through()
    {
        var (client, _) = await SignedIn();

        // rowVersion left null — a client that may skip it is a client that can silently overwrite a
        // change it never saw.
        var response = await client.PutAsJsonAsync(
            "/api/v1/users/me/preferences",
            new PreferencesBody { ReadingFontScale = 1.0 });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var reread = await ReadPreferences(client);
        Assert.Equal("en", reread.GetProperty("applicationLanguage").GetString());
    }

    [Fact]
    public async Task An_unsupported_language_is_refused()
    {
        var (client, _) = await SignedIn();
        var current = await ReadPreferences(client);

        var response = await client.PutAsJsonAsync(
            "/api/v1/users/me/preferences",
            ValidBody(current) with { ApplicationLanguage = "de" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
