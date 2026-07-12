namespace WhyStack.Domain.Identity;

/// <summary>
/// One refresh-token lifetime. The mechanism ADR-0008 calls a high-risk area lives here.
/// </summary>
/// <remarks>
/// <para><b>Rotation.</b> Every use of a refresh token issues a new one and retires the old. A stolen
/// token is therefore only useful until the legitimate client next refreshes — which is minutes, not
/// months.</para>
///
/// <para><b>Reuse detection.</b> Rotation alone does not tell you that a theft happened. If a token
/// that has ALREADY been rotated is presented again, exactly one of two things is true: the attacker
/// is replaying a stolen token, or the real user is replaying one — and both mean the token leaked.
/// The response is not to reject that one request. It is to revoke the entire <see cref="FamilyId"/>,
/// because the thief may hold a newer token in the same chain, and rejecting only the old one leaves
/// them logged in while the victim is not.</para>
///
/// <para>This is the whole reason <see cref="FamilyId"/> and <see cref="ReplacedBySessionId"/> exist.
/// Without them you can rotate, but you cannot detect, and rotation without detection buys far less
/// than it appears to.</para>
/// </remarks>
public class UserSession
{
    public Guid Id { get; init; }

    public required Guid UserId { get; init; }

    /// <summary>
    /// Every session descended from one sign-in shares this. Revoking a family kills the whole chain,
    /// which is the only response to a replay that actually locks the attacker out.
    /// </summary>
    public required Guid FamilyId { get; init; }

    /// <summary>
    /// SHA-256 of the refresh token. The raw token is returned to the client once and never stored
    /// (ADR-0008, `07`, `12`). A database dump therefore yields nobody a session.
    /// </summary>
    public required string RefreshTokenHash { get; init; }

    /// <summary>Set when this token is rotated. Its presence is what makes a later use a REPLAY.</summary>
    public Guid? ReplacedBySessionId { get; set; }

    /// <summary>Coarse. "Android", "iOS", "Web" — enough to show the user their sessions, and no more.</summary>
    public string? Platform { get; init; }

    public string? DeviceType { get; init; }

    /// <summary>
    /// Hashed, never raw (`07`: "Sensitive metadata should be privacy-reviewed"). An IP address is
    /// personal data; its hash is enough to notice that a session moved continents, which is all we
    /// want it for.
    /// </summary>
    public string? IpAddressHash { get; init; }

    public string? UserAgentHash { get; init; }

    public DateTime CreatedAtUtc { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
    public DateTime? LastUsedAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }

    /// <summary>Why it was revoked. A support question a year from now is answered by this column.</summary>
    public SessionRevocationReason? RevocationReason { get; set; }

    public bool IsRevoked => RevokedAtUtc is not null;

    public bool IsExpired(DateTime nowUtc) => ExpiresAtUtc <= nowUtc;

    /// <summary>A token that has been rotated. Presenting it again is a replay, not a mistake.</summary>
    public bool IsRotated => ReplacedBySessionId is not null;

    public bool IsUsable(DateTime nowUtc) => !IsRevoked && !IsRotated && !IsExpired(nowUtc);
}

public enum SessionRevocationReason
{
    /// <summary>The user signed out.</summary>
    Logout = 1,

    /// <summary>A rotated token was presented again. The whole family dies (ADR-0008).</summary>
    ReuseDetected = 2,

    /// <summary>The user changed or reset their password. Every session ends.</summary>
    PasswordChanged = 3,

    /// <summary>An administrator revoked it.</summary>
    AdminRevoked = 4,

    /// <summary>The user signed out of every device.</summary>
    LogoutAllDevices = 5,
}
