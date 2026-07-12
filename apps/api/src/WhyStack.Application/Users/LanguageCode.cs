namespace WhyStack.Application.Users;

/// <summary>
/// The two language axes, and the rule that maps a device locale onto one of them.
/// </summary>
/// <remarks>
/// `04`: "Turkish device language sets the application language to Turkish. All other device languages
/// default to English. The user may change the application language independently. The user may change
/// the content language independently."
///
/// The client already resolves this from the device locale — but the API must too. A rule that lives
/// only in the client is a rule the next caller does not have, and the next caller is the one that
/// sends <c>"de-DE"</c> and gets a 500 or, worse, an account whose interface language is German and
/// whose translations do not exist.
/// </remarks>
public static class LanguageCode
{
    public const string English = "en";
    public const string Turkish = "tr";

    public static readonly IReadOnlySet<string> Supported = new HashSet<string>(StringComparer.Ordinal)
    {
        English,
        Turkish,
    };

    public static bool IsSupported(string? code) =>
        code is not null && Supported.Contains(code);

    /// <summary>
    /// Maps a device locale ("tr-TR", "tr", "en-GB", "de-DE") onto a supported language.
    ///
    /// It never throws and never rejects. An unsupported device locale is a normal thing for a person
    /// to have, not an error — and refusing to register someone because their phone is in German would
    /// be an absurd way to enforce a language list.
    /// </summary>
    public static string FromDeviceLocale(string? deviceLocale)
    {
        if (string.IsNullOrWhiteSpace(deviceLocale))
        {
            return English;
        }

        // "tr-TR" and "tr_TR" both appear in the wild, depending on platform.
        var primary = deviceLocale.Split('-', '_')[0].Trim().ToLowerInvariant();

        return primary == Turkish ? Turkish : English;
    }
}
