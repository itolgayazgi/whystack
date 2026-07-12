using WhyStack.Application.Common;
using WhyStack.Application.Tests.Identity;
using WhyStack.Application.Users.Preferences;
using WhyStack.Domain.Users;

namespace WhyStack.Application.Tests.Users;

public class PreferencesHandlerTests
{
    private static readonly DateTime Now = new(2026, 7, 12, 9, 0, 0, DateTimeKind.Utc);

    private readonly FakeUserPreferencesRepository _repository = new();
    private readonly FakeClock _clock = new(Now);
    private readonly Guid _userId = Guid.CreateVersion7();

    private UpdatePreferencesHandler Update() => new(_repository, _clock);

    private GetPreferencesHandler Get() => new(_repository);

    /// <summary>A complete, valid body. Each test then breaks exactly one thing.</summary>
    private UpdatePreferencesCommand ValidCommand(string rowVersion) =>
        new(
            _userId,
            ApplicationLanguage: "tr",
            ContentLanguage: "en",
            ThemeMode: ThemeMode.Dark,
            ReadingFontScale: 1.25,
            ReducedMotionEnabled: true,
            PreferredSkillLevel: SkillLevel.MidLevel,
            RowVersion: rowVersion);

    private string SeedAndGetRowVersion()
    {
        var seeded = _repository.Seed(_userId, Now);

        return Convert.ToBase64String(seeded.RowVersion!);
    }

    [Fact]
    public async Task Saves_every_field()
    {
        var rowVersion = SeedAndGetRowVersion();

        var result = await Update().HandleAsync(ValidCommand(rowVersion), default);

        Assert.True(result.IsSuccess);

        var stored = _repository.Preferences.Single();
        Assert.Equal("tr", stored.ApplicationLanguageCode);
        Assert.Equal("en", stored.ContentLanguageCode);
        Assert.Equal(ThemeMode.Dark, stored.ThemeMode);
        Assert.Equal(1.25, stored.ReadingFontScale);
        Assert.True(stored.ReducedMotionEnabled);
        Assert.Equal(SkillLevel.MidLevel, stored.PreferredSkillLevel);
        Assert.Equal(Now, stored.UpdatedAtUtc);
    }

    /// <summary>
    /// The two language axes are INDEPENDENT (`07`). This is the test that would fail if somebody ever
    /// "simplified" them into one column, or made one of them follow the other.
    /// </summary>
    [Fact]
    public async Task The_interface_language_and_the_content_language_do_not_have_to_match()
    {
        var rowVersion = SeedAndGetRowVersion();

        // A Turkish developer, reading the English original.
        var result = await Update().HandleAsync(
            ValidCommand(rowVersion) with { ApplicationLanguage = "tr", ContentLanguage = "en" },
            default);

        Assert.True(result.IsSuccess);
        Assert.Equal("tr", result.Value.ApplicationLanguage);
        Assert.Equal("en", result.Value.ContentLanguage);
    }

    /// <summary>
    /// The point of the whole rowVersion mechanism, and the one test that can falsify it.
    ///
    /// Two devices load the same preferences. Both change something. The second one to save is told NO
    /// — because if it were not, it would silently revert the first, and the only person who would ever
    /// notice is the user, weeks later, wondering why the setting they changed keeps coming back.
    /// </summary>
    [Fact]
    public async Task A_second_writer_who_started_from_a_stale_copy_is_rejected()
    {
        var staleRowVersion = SeedAndGetRowVersion();

        // The phone saves. It wins, and the row's version moves on.
        var first = await Update().HandleAsync(
            ValidCommand(staleRowVersion) with { ThemeMode = ThemeMode.Dark },
            default);

        Assert.True(first.IsSuccess);

        // The laptop saves, still holding the version it loaded a minute ago.
        var second = await Update().HandleAsync(
            ValidCommand(staleRowVersion) with { ThemeMode = ThemeMode.Light },
            default);

        Assert.False(second.IsSuccess);
        Assert.Equal(ErrorCodes.ConcurrencyConflict, second.Error!.Code);

        // And — the part that actually matters — the phone's change is still there.
        Assert.Equal(ThemeMode.Dark, _repository.Preferences.Single().ThemeMode);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task The_rowVersion_from_the_last_read_lets_the_next_write_through()
    {
        var rowVersion = SeedAndGetRowVersion();

        var first = await Update().HandleAsync(ValidCommand(rowVersion), default);
        Assert.True(first.IsSuccess);

        // Re-reading gives a fresh token, and the write goes through. Otherwise the conflict check would
        // be indistinguishable from a bug that rejects every second write.
        var second = await Update().HandleAsync(
            ValidCommand(first.Value.RowVersion) with { ThemeMode = ThemeMode.Light },
            default);

        Assert.True(second.IsSuccess);
        Assert.Equal(ThemeMode.Light, _repository.Preferences.Single().ThemeMode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-base64!!")]
    [InlineData("YWJj")]  // valid base64, but three bytes. A rowversion is eight.
    public async Task Refuses_to_write_without_a_usable_rowVersion(string? rowVersion)
    {
        SeedAndGetRowVersion();

        var result = await Update().HandleAsync(ValidCommand(rowVersion!), default);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ValidationFailed, result.Error!.Code);
        Assert.Contains("rowVersion", result.Error.FieldErrors!.Keys);

        // Nothing was written. A missing token must not fall back to "just save it" — that fallback IS
        // the lost update the mechanism exists to prevent.
        Assert.Equal(0, _repository.SaveCount);
    }

    [Theory]
    [InlineData(1.1)]     // between steps
    [InlineData(500.0)]   // the one that breaks every screen
    [InlineData(0.0)]
    [InlineData(null)]
    public async Task Refuses_a_reading_scale_that_is_not_a_token_step(double? scale)
    {
        var rowVersion = SeedAndGetRowVersion();

        var result = await Update().HandleAsync(
            ValidCommand(rowVersion) with { ReadingFontScale = scale },
            default);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ValidationFailed, result.Error!.Code);
        Assert.Contains("readingFontScale", result.Error.FieldErrors!.Keys);

        // Not clamped, not snapped to the nearest step, not silently corrected. The caller is told.
        Assert.Equal(0, _repository.SaveCount);
    }

    [Theory]
    [InlineData("de")]
    [InlineData("tr-TR")]  // a locale, not a language code
    [InlineData("")]
    [InlineData(null)]
    public async Task Refuses_an_unsupported_language(string? language)
    {
        var rowVersion = SeedAndGetRowVersion();

        var application = await Update().HandleAsync(
            ValidCommand(rowVersion) with { ApplicationLanguage = language }, default);

        var content = await Update().HandleAsync(
            ValidCommand(rowVersion) with { ContentLanguage = language }, default);

        Assert.Contains("applicationLanguage", application.Error!.FieldErrors!.Keys);
        Assert.Contains("contentLanguage", content.Error!.FieldErrors!.Keys);
    }

    /// <summary>
    /// `08`: PUT is a full replacement. An omitted field is an omission — it is NOT an instruction to
    /// keep the current value.
    /// </summary>
    [Fact]
    public async Task An_incomplete_body_is_rejected_rather_than_merged()
    {
        var rowVersion = SeedAndGetRowVersion();

        var result = await Update().HandleAsync(
            new UpdatePreferencesCommand(_userId, "en", "en", null, null, null, null, rowVersion),
            default);

        Assert.False(result.IsSuccess);

        // Every missing field is named, in one response. Reporting them one at a time would make the
        // client fix, resubmit, fix, resubmit — and `08` asks for the full set.
        Assert.Equal(
            ["readingFontScale", "reducedMotionEnabled", "themeMode"],
            result.Error!.FieldErrors!.Keys.Order());
    }

    [Fact]
    public async Task Not_stating_a_skill_level_is_a_valid_answer()
    {
        var rowVersion = SeedAndGetRowVersion();

        var result = await Update().HandleAsync(
            ValidCommand(rowVersion) with { PreferredSkillLevel = null },
            default);

        Assert.True(result.IsSuccess);
        Assert.Null(_repository.Preferences.Single().PreferredSkillLevel);
    }

    [Fact]
    public async Task Reading_preferences_that_do_not_exist_is_a_404_and_never_creates_them()
    {
        var result = await Get().HandleAsync(_userId, default);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ResourceNotFound, result.Error!.Code);

        // `08`: a GET must not mutate server state. And a GET that quietly repaired the missing row
        // would hide the fact that registration had stopped writing preferences at all.
        Assert.Empty(_repository.Preferences);
    }

    [Fact]
    public async Task Reads_back_what_was_written()
    {
        var rowVersion = SeedAndGetRowVersion();
        await Update().HandleAsync(ValidCommand(rowVersion), default);

        var result = await Get().HandleAsync(_userId, default);

        Assert.True(result.IsSuccess);
        Assert.Equal("tr", result.Value.ApplicationLanguage);
        Assert.Equal(ThemeMode.Dark, result.Value.ThemeMode);
        Assert.Equal(1.25, result.Value.ReadingFontScale);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.RowVersion));
    }
}
