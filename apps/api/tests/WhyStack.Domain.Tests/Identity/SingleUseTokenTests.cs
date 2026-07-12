using WhyStack.Domain.Identity;

namespace WhyStack.Domain.Tests.Identity;

/// <summary>
/// A password-reset token is a temporary password. It gets the same scrutiny as one.
/// </summary>
public class SingleUseTokenTests
{
    private static readonly DateTime Now = new(2026, 7, 12, 12, 0, 0, DateTimeKind.Utc);

    private static PasswordResetToken Token() => new()
    {
        Id = Guid.CreateVersion7(),
        UserId = Guid.CreateVersion7(),
        TokenHash = new string('b', 64),
        CreatedAtUtc = Now,
        ExpiresAtUtc = Now.AddHours(1),
    };

    [Fact]
    public void A_fresh_token_can_be_redeemed()
    {
        Assert.True(Token().IsRedeemable(Now));
    }

    /// <summary>
    /// Single use is not a nicety. A reset link sits in an inbox — and inboxes get breached, forwarded,
    /// synced to a laptop somebody later sells. Without this, that link stays a working key to the
    /// account for as long as the mail exists.
    /// </summary>
    [Fact]
    public void A_redeemed_token_cannot_be_redeemed_again()
    {
        var token = Token();
        token.UsedAtUtc = Now;

        Assert.True(token.IsUsed);
        Assert.False(token.IsRedeemable(Now));
    }

    [Fact]
    public void An_expired_token_cannot_be_redeemed()
    {
        var token = Token();

        Assert.False(token.IsRedeemable(token.ExpiresAtUtc));
        Assert.True(token.IsRedeemable(token.ExpiresAtUtc.AddTicks(-1)));
    }

    /// <summary>
    /// A used token stays dead even if the clock somehow rolls back. Belt and braces: the two guards
    /// are independent, and a bug in one must not open the other.
    /// </summary>
    [Fact]
    public void A_used_token_stays_dead_regardless_of_time()
    {
        var token = Token();
        token.UsedAtUtc = Now;

        Assert.False(token.IsRedeemable(Now.AddYears(-1)));
    }

    [Fact]
    public void An_email_confirmation_token_remembers_the_address_it_confirms()
    {
        var token = new EmailConfirmationToken
        {
            Id = Guid.CreateVersion7(),
            UserId = Guid.CreateVersion7(),
            TokenHash = new string('c', 64),
            Email = "ada@example.com",
            CreatedAtUtc = Now,
            ExpiresAtUtc = Now.AddDays(1),
        };

        // It must confirm the address it was SENT to — not whatever the user row says by the time they
        // click it. Otherwise changing the email after requesting confirmation confirms the new one
        // with a link mailed to the old.
        Assert.Equal("ada@example.com", token.Email);
        Assert.True(token.IsRedeemable(Now));
    }
}
