using WhyStack.Application.Users;

namespace WhyStack.Application.Tests.Users;

/// <summary>`04` — Device Language Detection. Turkish device → Turkish. Everything else → English.</summary>
public class LanguageCodeTests
{
    [Theory]
    [InlineData("tr")]
    [InlineData("tr-TR")]
    [InlineData("tr-CY")]      // Turkish, in Cyprus. Still Turkish.
    [InlineData("tr_TR")]      // Android reports underscores; iOS reports hyphens. Both are real.
    [InlineData("TR-tr")]      // Locale tags are case-insensitive, and something, somewhere, shouts.
    [InlineData("  tr-TR  ")]
    public void A_Turkish_device_gets_Turkish(string locale) =>
        Assert.Equal("tr", LanguageCode.FromDeviceLocale(locale));

    [Theory]
    [InlineData("en")]
    [InlineData("en-GB")]
    [InlineData("de-DE")]
    [InlineData("az-AZ")]      // Azerbaijani is close to Turkish. It is not Turkish.
    [InlineData("ku-TR")]      // Kurdish, on a phone bought in Turkey. The LANGUAGE decides, not the region.
    public void Every_other_device_gets_English(string locale) =>
        Assert.Equal("en", LanguageCode.FromDeviceLocale(locale));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nonsense")]
    public void A_missing_or_unparseable_locale_gets_English_and_never_throws(string? locale)
    {
        // Not an error, and never a rejected registration. A phone with a locale we do not recognise is
        // a normal thing for a person to own, and refusing to create their account over it would be an
        // absurd way to enforce a language list.
        Assert.Equal("en", LanguageCode.FromDeviceLocale(locale));
    }

    [Fact]
    public void Only_the_two_supported_languages_are_supported()
    {
        Assert.True(LanguageCode.IsSupported("en"));
        Assert.True(LanguageCode.IsSupported("tr"));

        Assert.False(LanguageCode.IsSupported("de"));
        Assert.False(LanguageCode.IsSupported("EN"));   // stored lowercase; the wire is exact
        Assert.False(LanguageCode.IsSupported("tr-TR")); // a LOCALE, not a language code
        Assert.False(LanguageCode.IsSupported(null));
    }
}
