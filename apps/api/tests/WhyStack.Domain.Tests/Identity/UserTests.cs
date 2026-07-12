using WhyStack.Domain.Identity;

namespace WhyStack.Domain.Tests.Identity;

/// <summary>
/// These run with no database and no host, because <see cref="User"/> depends on nothing. That is the
/// point of the layer: the rules that decide whether somebody may sign in are testable in microseconds,
/// so there is no excuse for not testing every branch of them.
/// </summary>
public class UserTests
{
    private static readonly DateTime Now = new(2026, 7, 12, 12, 0, 0, DateTimeKind.Utc);

    private static User Account() => new()
    {
        Id = Guid.CreateVersion7(),
        Email = "ada@example.com",
        NormalizedEmail = "ADA@EXAMPLE.COM",
        PasswordHash = "hash",
        CreatedAtUtc = Now,
    };

    [Fact]
    public void A_normal_account_can_sign_in()
    {
        Assert.True(Account().CanSignIn(Now));
    }

    [Fact]
    public void A_deactivated_account_cannot_sign_in()
    {
        var user = Account();
        user.IsActive = false;

        Assert.False(user.CanSignIn(Now));
    }

    [Fact]
    public void A_soft_deleted_account_cannot_sign_in()
    {
        var user = Account();
        user.DeletedAtUtc = Now.AddDays(-1);

        Assert.True(user.IsDeleted);
        Assert.False(user.CanSignIn(Now));
    }

    [Fact]
    public void A_locked_account_cannot_sign_in_while_the_lock_holds()
    {
        var user = Account();
        user.IsLocked = true;
        user.LockedUntilUtc = Now.AddMinutes(5);

        Assert.False(user.CanSignIn(Now));
    }

    /// <summary>
    /// The lock must lift on its own. A lockout that needs an administrator to clear it is a denial of
    /// service anyone can trigger against any account, for free, by typing the wrong password five
    /// times — and the victim is the one locked out, not the attacker.
    /// </summary>
    [Fact]
    public void A_lock_expires_by_itself()
    {
        var user = Account();
        user.IsLocked = true;
        user.LockedUntilUtc = Now.AddMinutes(-1);

        Assert.False(user.IsLockedAt(Now));
        Assert.True(user.CanSignIn(Now));
    }

    /// <summary>
    /// A lock with no expiry is permanent — and that is a decision only an administrator makes, never
    /// a failed-password counter. Encoded here so nobody sets IsLocked without deciding which they mean.
    /// </summary>
    [Fact]
    public void A_lock_with_no_expiry_is_permanent()
    {
        var user = Account();
        user.IsLocked = true;
        user.LockedUntilUtc = null;

        Assert.True(user.IsLockedAt(Now.AddYears(10)));
        Assert.False(user.CanSignIn(Now.AddYears(10)));
    }

    [Fact]
    public void The_lock_boundary_is_exclusive_at_the_expiry_instant()
    {
        var user = Account();
        user.IsLocked = true;
        user.LockedUntilUtc = Now;

        // At exactly the expiry instant the lock is over. Off-by-one here is a second of free guessing.
        Assert.False(user.IsLockedAt(Now));
    }
}
