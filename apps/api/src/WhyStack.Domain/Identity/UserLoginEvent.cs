namespace WhyStack.Domain.Identity;

/// <summary>
/// The audit trail for identity events (`07`, ADR-0008).
/// </summary>
/// <remarks>
/// It answers questions nothing else can: is this account being brute-forced, did the password reset
/// the user swears they never asked for actually happen, when did this session really start.
///
/// It also carries a hard rule. <b>No raw password, no raw token, ever</b> — not in a column, not in
/// <see cref="FailureReason"/>, not "just for debugging". An audit log is the one table most likely to
/// be read by someone who should not have it, and the one nobody thinks of as sensitive.
///
/// <see cref="Email"/> is stored even when <see cref="UserId"/> is null, and that is deliberate: a
/// login attempt against an address that does not exist is exactly the event worth seeing.
/// </remarks>
public class UserLoginEvent
{
    public Guid Id { get; init; }

    /// <summary>Null when the attempt was against an address with no account.</summary>
    public Guid? UserId { get; init; }

    public required string Email { get; init; }

    public required LoginEventType EventType { get; init; }

    /// <summary>
    /// `07` names this column <c>Succeeded</c>. The same document's boolean rule requires an
    /// <c>Is</c> prefix and calls bare participles bad. The rule wins over the example.
    /// </summary>
    public required bool IsSuccessful { get; init; }

    /// <summary>
    /// Coarse and safe to store. Never the credential, never a token, never a stack trace.
    /// A reason precise enough to be useful to an attacker is a reason precise enough to be an oracle.
    /// </summary>
    public string? FailureReason { get; init; }

    public string? IpAddressHash { get; init; }
    public string? UserAgentHash { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}

public enum LoginEventType
{
    RegistrationSucceeded = 1,
    LoginSucceeded = 2,
    LoginFailed = 3,
    Logout = 4,
    TokenRefreshed = 5,
    TokenRefreshFailed = 6,

    /// <summary>
    /// A rotated refresh token was presented again. This is the loudest event in the table: either a
    /// token leaked, or one is being replayed. The session family is revoked (ADR-0008).
    /// </summary>
    TokenReuseDetected = 7,

    PasswordResetRequested = 8,
    PasswordResetCompleted = 9,
    EmailConfirmed = 10,
    AccountLocked = 11,
}
