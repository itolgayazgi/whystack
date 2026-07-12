using System.Globalization;

namespace WhyStack.Application.Identity;

/// <summary>Email handling, in one place, so that normalisation cannot drift between callers.</summary>
public static class EmailAddress
{
    public const int MaximumLength = 256;

    /// <summary>
    /// <b>Invariant culture, always.</b> Not a style preference — a correctness one.
    ///
    /// In a Turkish culture, <c>"i".ToUpper()</c> is <c>"İ"</c>, not <c>"I"</c>. So
    /// <c>ada@ok.com</c> normalises to <c>ADA@OK.COM</c> on a machine with an invariant culture and to
    /// <c>ADA@OK.COM</c> with a dotted İ on a machine running in Turkish. The unique index would then
    /// see two different strings, and the same person would get two accounts — on a Turkish product,
    /// which is precisely where it would happen.
    ///
    /// This is the Turkish-I problem, and it is why the whole codebase uses ToUpperInvariant.
    /// </summary>
    public static string Normalize(string email) =>
        email.Trim().ToUpperInvariant();

    /// <summary>
    /// Deliberately permissive. A regex that tries to fully implement RFC 5322 is famously
    /// unreadable, still wrong, and rejects addresses that work — and the only test that actually
    /// proves an address is real is sending mail to it, which the confirmation flow does anyway.
    /// This rejects what is obviously not an address and lets the mail server decide the rest.
    /// </summary>
    public static bool LooksValid(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > MaximumLength)
        {
            return false;
        }

        var at = email.IndexOf('@', StringComparison.Ordinal);

        return at > 0
            && at == email.LastIndexOf('@')
            && at < email.Length - 1
            && !email.Contains(' ', StringComparison.Ordinal)
            && email.IndexOf('.', at) > at + 1;
    }

    public static string Format(string email) =>
        email.Trim().ToLower(CultureInfo.InvariantCulture);
}
