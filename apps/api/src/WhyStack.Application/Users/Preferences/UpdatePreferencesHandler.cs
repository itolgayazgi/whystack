using WhyStack.Application.Abstractions;
using WhyStack.Application.Common;
using WhyStack.Domain.Users;

namespace WhyStack.Application.Users.Preferences;

/// <summary>
/// Every field is nullable and every field is required. That is not a contradiction.
/// </summary>
/// <remarks>
/// Nullable because JSON can deliver <c>null</c> for anything, and a non-nullable property here would
/// be a lie the compiler believes. Required because `08` defines <c>PUT</c> as FULL REPLACEMENT: a
/// missing field is an omission, not an instruction to keep the old value.
///
/// The distinction matters. If a missing field silently meant "leave it alone", then a client with a
/// bug — or an older version that does not know about a newer setting — would look like it was working
/// while quietly never being able to change that setting. Rejecting the incomplete body says so out
/// loud, at the cost of one honest error.
///
/// <see cref="PreferredSkillLevel"/> is the exception, and genuinely optional: <c>null</c> is a real
/// answer there ("not stated"), not an absence.
/// </remarks>
public sealed record UpdatePreferencesCommand(
    Guid UserId,
    string? ApplicationLanguage,
    string? ContentLanguage,
    ThemeMode? ThemeMode,
    double? ReadingFontScale,
    bool? ReducedMotionEnabled,
    SkillLevel? PreferredSkillLevel,
    string? RowVersion);

public sealed class UpdatePreferencesHandler(IUserPreferencesRepository repository, IClock clock)
{
    /// <summary>A rowversion is exactly 8 bytes. Anything else was not one of ours.</summary>
    private const int RowVersionBytes = 8;

    public async Task<Result<UserPreferencesResult>> HandleAsync(
        UpdatePreferencesCommand command,
        CancellationToken cancellationToken)
    {
        var fieldErrors = new Dictionary<string, string[]>();

        if (!LanguageCode.IsSupported(command.ApplicationLanguage))
        {
            fieldErrors["applicationLanguage"] =
                [$"Must be one of: {string.Join(", ", LanguageCode.Supported)}."];
        }

        // The content language is validated against the SAME list, and that is a decision worth naming:
        // the two axes are independent (`07` — "Application language and content language must remain
        // separate"), but independent does not mean the sets differ. They will: content will exist in
        // languages the interface has never been translated into, and the interface may be translated
        // before any content follows it. When that day comes this line changes, and the fact that it is
        // a SEPARATE line is what makes that a one-line change instead of an untangling.
        if (!LanguageCode.IsSupported(command.ContentLanguage))
        {
            fieldErrors["contentLanguage"] =
                [$"Must be one of: {string.Join(", ", LanguageCode.Supported)}."];
        }

        if (command.ThemeMode is null)
        {
            fieldErrors["themeMode"] = ["A theme mode is required."];
        }

        if (command.ReadingFontScale is not { } scale || !ReadingFontScale.IsAllowed(scale))
        {
            // Not clamped, not snapped to the nearest step. A silent correction would leave the client
            // believing it had set 1.1 while the server stored 1.0, and nothing anywhere would say so.
            fieldErrors["readingFontScale"] = [$"Must be one of: {ReadingFontScale.Describe()}."];
        }

        if (command.ReducedMotionEnabled is null)
        {
            fieldErrors["reducedMotionEnabled"] = ["A value is required."];
        }

        var expectedRowVersion = DecodeRowVersion(command.RowVersion);

        if (expectedRowVersion is null)
        {
            // Required, and that is the point of the whole mechanism: to write, you must first have
            // read. A client that may omit it is a client that can overwrite a change it never saw.
            fieldErrors["rowVersion"] =
                ["The rowVersion from a preferences read is required. Read before you write."];
        }

        if (fieldErrors.Count > 0)
        {
            return Error.Validation(fieldErrors);
        }

        var preferences = await repository.FindByUserIdAsync(command.UserId, cancellationToken);

        if (preferences is null)
        {
            return new Error(ErrorCodes.ResourceNotFound, "No preferences exist for this account.");
        }

        preferences.ApplicationLanguageCode = command.ApplicationLanguage!;
        preferences.ContentLanguageCode = command.ContentLanguage!;
        preferences.ThemeMode = command.ThemeMode!.Value;
        preferences.ReadingFontScale = command.ReadingFontScale!.Value;
        preferences.ReducedMotionEnabled = command.ReducedMotionEnabled!.Value;
        preferences.PreferredSkillLevel = command.PreferredSkillLevel;
        preferences.UpdatedAtUtc = clock.UtcNow;

        var saved = await repository.TrySaveAsync(preferences, expectedRowVersion!, cancellationToken);

        if (!saved)
        {
            // The other device won. We do NOT merge, and we do not retry: this is a settings screen, and
            // the only person who can say which of two conflicting intentions is the right one is the
            // human who expressed them. Re-read, show them what it says now, let them decide.
            return new Error(
                ErrorCodes.ConcurrencyConflict,
                "These preferences were changed somewhere else after you loaded them. "
                + "Reload them and apply your change again.");
        }

        return Result<UserPreferencesResult>.Success(UserPreferencesResult.From(preferences));
    }

    private static byte[]? DecodeRowVersion(string? encoded)
    {
        if (string.IsNullOrWhiteSpace(encoded))
        {
            return null;
        }

        // TryFromBase64String, not Convert.FromBase64String: a client sending junk is a 422, not an
        // unhandled FormatException and a 500.
        var buffer = new byte[RowVersionBytes];

        return Convert.TryFromBase64String(encoded, buffer, out var written) && written == RowVersionBytes
            ? buffer
            : null;
    }
}
