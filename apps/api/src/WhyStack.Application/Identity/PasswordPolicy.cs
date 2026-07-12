namespace WhyStack.Application.Identity;

/// <summary>
/// What counts as an acceptable password.
/// </summary>
/// <remarks>
/// `04` requires "secure password handling" and does not say what that means, so this is a decision,
/// and it follows NIST SP 800-63B rather than habit.
///
/// <b>Length is the requirement. Composition is not.</b> There is no "one uppercase, one digit, one
/// symbol" rule here, and its absence is deliberate: those rules do not produce strong passwords, they
/// produce <c>Password1!</c>. They push people toward a predictable pattern — capitalise the first
/// letter, append a digit and a bang — which an attacker's rules engine models in about four lines.
/// Meanwhile they make good passphrases fail, so people write the password down.
///
/// <b>A maximum exists, and it is not about strength.</b> Hashing is deliberately expensive; that is
/// the point of it. A megabyte-long password is a free way to make the server burn CPU on every login
/// attempt, and enough of them at once is a denial of service that costs the attacker nothing.
/// 256 is far beyond any real passphrase and far below anything that hurts.
/// </remarks>
public static class PasswordPolicy
{
    /// <summary>
    /// NIST's floor for a user-chosen secret is 8. This is 10, because we do not (yet) check candidates
    /// against a breached-password list, and length is the only compensating control we have.
    /// </summary>
    public const int MinimumLength = 10;

    /// <summary>Not a strength limit. A hashing-cost limit — see the remarks on the class.</summary>
    public const int MaximumLength = 256;

    public static IReadOnlyList<string> Validate(string? password, string email)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            failures.Add("Password is required.");
            return failures;
        }

        if (password.Length < MinimumLength)
        {
            failures.Add($"Password must be at least {MinimumLength} characters.");
        }

        if (password.Length > MaximumLength)
        {
            failures.Add($"Password must be at most {MaximumLength} characters.");
        }

        // The single most guessable password for any given account is the account's own address.
        if (!string.IsNullOrWhiteSpace(email)
            && password.Contains(email, StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("Password must not contain your email address.");
        }

        return failures;
    }
}
