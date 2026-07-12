using WhyStack.Domain.Users;

namespace WhyStack.Application.Users.Preferences;

/// <summary>
/// One learner's settings, as the API returns them.
/// </summary>
/// <param name="RowVersion">
/// An opaque concurrency token. The client does not read it — it sends it back on the next write, and
/// that is what lets the server tell "you are updating what you saw" from "somebody changed this
/// underneath you".
/// </param>
public sealed record UserPreferencesResult(
    string ApplicationLanguage,
    string ContentLanguage,
    ThemeMode ThemeMode,
    double ReadingFontScale,
    bool ReducedMotionEnabled,
    SkillLevel? PreferredSkillLevel,
    string RowVersion)
{
    public static UserPreferencesResult From(UserPreferences preferences) =>
        new(
            preferences.ApplicationLanguageCode,
            preferences.ContentLanguageCode,
            preferences.ThemeMode,
            preferences.ReadingFontScale,
            preferences.ReducedMotionEnabled,
            preferences.PreferredSkillLevel,
            // Base64, because a rowversion is 8 raw bytes and JSON has no byte type. Encoding it as a
            // string keeps it opaque, which is the point: the moment a client tries to interpret it,
            // the server loses the freedom to change how it is produced.
            Convert.ToBase64String(preferences.RowVersion ?? []));
}
