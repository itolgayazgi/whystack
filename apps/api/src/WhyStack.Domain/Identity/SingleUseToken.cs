namespace WhyStack.Domain.Identity;

/// <summary>
/// Shared shape of the email-confirmation and password-reset tokens (`07`).
/// </summary>
/// <remarks>
/// Both are the same object with a different purpose, and both get the same three properties wrong in
/// the same three ways if you are not careful:
///
/// <list type="number">
/// <item><b>Stored as a hash, never raw.</b> Whoever reads the table must not be able to use what they
/// find. A password-reset token in plaintext is a password.</item>
/// <item><b>Single use.</b> <see cref="UsedAtUtc"/> is set on redemption, and a second attempt fails.
/// Without it, a reset link in a mailbox stays a working key forever.</item>
/// <item><b>Expiring.</b> A token that never expires is a credential nobody remembers issuing.</item>
/// </list>
/// </remarks>
public abstract class SingleUseToken
{
    public Guid Id { get; init; }

    public required Guid UserId { get; init; }

    /// <summary>SHA-256 of the token that was emailed. The raw value exists only in that email.</summary>
    public required string TokenHash { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public required DateTime ExpiresAtUtc { get; init; }

    /// <summary>Set on redemption. Non-null means spent.</summary>
    public DateTime? UsedAtUtc { get; set; }

    public bool IsUsed => UsedAtUtc is not null;

    public bool IsExpired(DateTime nowUtc) => ExpiresAtUtc <= nowUtc;

    public bool IsRedeemable(DateTime nowUtc) => !IsUsed && !IsExpired(nowUtc);
}

/// <summary>Proves the address on an account is reachable by the person who claimed it.</summary>
public class EmailConfirmationToken : SingleUseToken
{
    /// <summary>
    /// The address this token confirms. Kept because a user may change their email before redeeming,
    /// and a token must confirm the address it was sent to — not whatever the row says today.
    /// </summary>
    public required string Email { get; init; }
}

/// <summary>Lets someone who has lost their password set a new one, once, soon.</summary>
public class PasswordResetToken : SingleUseToken;
