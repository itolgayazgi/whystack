using WhyStack.Domain.Identity;

namespace WhyStack.Domain.Tests.Identity;

/// <summary>
/// The refresh-token rules. ADR-0008 calls this the high-risk area, and `13` classifies it as such,
/// so every state a session can be in gets a test — not just the happy one.
/// </summary>
public class UserSessionTests
{
    private static readonly DateTime Now = new(2026, 7, 12, 12, 0, 0, DateTimeKind.Utc);

    private static UserSession Session() => new()
    {
        Id = Guid.CreateVersion7(),
        UserId = Guid.CreateVersion7(),
        FamilyId = Guid.CreateVersion7(),
        RefreshTokenHash = new string('a', 64),
        CreatedAtUtc = Now,
        ExpiresAtUtc = Now.AddDays(30),
    };

    [Fact]
    public void A_fresh_session_is_usable()
    {
        Assert.True(Session().IsUsable(Now));
    }

    [Fact]
    public void A_revoked_session_is_not_usable()
    {
        var session = Session();
        session.RevokedAtUtc = Now;
        session.RevocationReason = SessionRevocationReason.Logout;

        Assert.True(session.IsRevoked);
        Assert.False(session.IsUsable(Now));
    }

    [Fact]
    public void An_expired_session_is_not_usable()
    {
        var session = Session();

        Assert.True(session.IsExpired(session.ExpiresAtUtc));
        Assert.False(session.IsUsable(session.ExpiresAtUtc));
    }

    [Fact]
    public void Expiry_is_inclusive_at_the_instant_itself()
    {
        var session = Session();

        // At exactly ExpiresAtUtc the token is dead. Getting this backwards hands an attacker a window
        // whose width is however long a refresh takes.
        Assert.False(session.IsUsable(session.ExpiresAtUtc));
        Assert.True(session.IsUsable(session.ExpiresAtUtc.AddTicks(-1)));
    }

    /// <summary>
    /// The one that matters. A rotated token has already been exchanged for a newer one, so presenting
    /// it again means one of exactly two things: an attacker is replaying a stolen token, or the real
    /// user is replaying one they should no longer have. Both mean it leaked.
    ///
    /// If <see cref="UserSession.IsUsable"/> ever forgets <see cref="UserSession.IsRotated"/>, a stolen
    /// refresh token works forever and nothing anywhere notices. That is the entire attack this design
    /// exists to stop, and this test is what keeps it stopped.
    /// </summary>
    [Fact]
    public void A_rotated_session_is_not_usable_and_using_it_again_is_a_replay()
    {
        var session = Session();
        session.ReplacedBySessionId = Guid.CreateVersion7();

        Assert.True(session.IsRotated);
        Assert.False(session.IsUsable(Now));
    }

    [Fact]
    public void A_revocation_records_why_it_happened()
    {
        var session = Session();
        session.RevokedAtUtc = Now;
        session.RevocationReason = SessionRevocationReason.ReuseDetected;

        // A year from now, "why was I signed out?" has an answer that is not a guess.
        Assert.Equal(SessionRevocationReason.ReuseDetected, session.RevocationReason);
    }
}
