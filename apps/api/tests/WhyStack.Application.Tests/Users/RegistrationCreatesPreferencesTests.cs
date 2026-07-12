using WhyStack.Application.Identity.Register;
using WhyStack.Application.Identity.Tokens;
using WhyStack.Application.Tests.Identity;
using WhyStack.Domain.Users;

namespace WhyStack.Application.Tests.Users;

/// <summary>
/// Registration must create the preferences row, in its own transaction, with the device's language.
/// </summary>
/// <remarks>
/// The invariant is "every user has exactly one preferences row". It is not decoration: `08` forbids a
/// GET from mutating server state, so <c>GET /users/me/preferences</c> cannot create the row it is
/// missing — which means an account without one would get a 404 on a screen that must always work.
/// </remarks>
public class RegistrationCreatesPreferencesTests
{
    private static readonly DateTime Now = new(2026, 7, 12, 9, 0, 0, DateTimeKind.Utc);

    private readonly FakeIdentityRepository _repository = new();
    private readonly FakeEmailSender _emails = new();
    private readonly FakeClock _clock = new(Now);

    private RegisterUserHandler Handler() =>
        new(
            _repository,
            new FakePasswordHasher(),
            new SingleUseTokenService(_repository, new FakeTokenGenerator(), new FakeTokenHasher(), _clock),
            _emails,
            new FakeAppLinks(),
            _clock);

    private async Task<UserPreferences> RegisterWith(string? deviceLocale)
    {
        var result = await Handler().HandleAsync(
            new RegisterUserCommand(
                "ada@example.com",
                "a-good-long-password",
                "Ada",
                IpAddressHash: null,
                UserAgentHash: null,
                DeviceLocale: deviceLocale),
            default);

        Assert.True(result.IsSuccess);

        return Assert.Single(_repository.Preferences);
    }

    [Fact]
    public async Task A_Turkish_device_starts_the_account_in_Turkish()
    {
        var preferences = await RegisterWith("tr-TR");

        Assert.Equal("tr", preferences.ApplicationLanguageCode);
    }

    [Theory]
    [InlineData("en-GB")]
    [InlineData("de-DE")]
    [InlineData(null)]
    public async Task Every_other_device_starts_the_account_in_English(string? locale)
    {
        var preferences = await RegisterWith(locale);

        Assert.Equal("en", preferences.ApplicationLanguageCode);
    }

    [Fact]
    public async Task The_defaults_are_the_ones_the_settings_screen_would_accept_back()
    {
        var preferences = await RegisterWith("tr-TR");

        Assert.Equal(ThemeMode.System, preferences.ThemeMode);
        Assert.Equal(WhyStack.Application.Users.ReadingFontScale.Default, preferences.ReadingFontScale);
        Assert.False(preferences.ReducedMotionEnabled);
        Assert.Null(preferences.PreferredSkillLevel);

        // A default outside the allowed set would mean every new account is created holding a value its
        // own PUT would reject with a 422 — which is the sort of thing nobody finds until a user tries
        // to change one unrelated setting and cannot save the form.
        Assert.True(WhyStack.Application.Users.ReadingFontScale.IsAllowed(preferences.ReadingFontScale));
    }

    /// <summary>
    /// Content starts in the same language as the interface — a first guess, not a coupling. The two
    /// axes are independent (`07`) and go their own ways the moment the user says so.
    /// </summary>
    [Fact]
    public async Task Content_starts_in_the_interface_language()
    {
        var preferences = await RegisterWith("tr-TR");

        Assert.Equal("tr", preferences.ContentLanguageCode);
    }

    /// <summary>
    /// The preferences row and the user are written by ONE SaveChanges — one transaction.
    ///
    /// If the role assignment or the confirmation token failed, there must be no user AND no preferences
    /// row. A user who exists with no preferences is an account whose settings screen 404s, and nobody
    /// would ever work out why.
    /// </summary>
    [Fact]
    public async Task The_user_and_the_preferences_are_saved_together()
    {
        await RegisterWith("tr-TR");

        Assert.Equal(1, _repository.SaveCount);
        Assert.Single(_repository.Users);
        Assert.Equal(_repository.Users.Single().Id, _repository.Preferences.Single().UserId);
    }

    /// <summary>An address that is already taken creates nothing — including no second preferences row.</summary>
    [Fact]
    public async Task Registering_a_taken_address_does_not_create_a_second_preferences_row()
    {
        await RegisterWith("tr-TR");

        var again = await Handler().HandleAsync(
            new RegisterUserCommand("ada@example.com", "a-good-long-password", "Ada", null, null, "en-GB"),
            default);

        // Same indistinguishable answer as a real registration — and no new row.
        Assert.True(again.IsSuccess);
        Assert.Single(_repository.Preferences);
        Assert.Equal("tr", _repository.Preferences.Single().ApplicationLanguageCode);
    }
}
