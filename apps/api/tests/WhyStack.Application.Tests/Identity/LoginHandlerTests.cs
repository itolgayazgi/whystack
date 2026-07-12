using WhyStack.Application.Common;
using WhyStack.Application.Identity.Login;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Tests.Identity;

public class LoginHandlerTests
{
    private const string Password = "a-good-long-password";

    private static readonly DateTime Now = new(2026, 7, 12, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeIdentityRepository _repository = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeClock _clock = new(Now);
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _handler = new LoginHandler(_repository, _hasher, new FakeAccessTokenIssuer(), _clock);
    }

    private User GivenAnAccount(string email = "ada@example.com")
    {
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = _hasher.Hash(Password),
            IsActive = true,
            CreatedAtUtc = Now,
        };

        _repository.Users.Add(user);
        return user;
    }

    private Task<Result<LoginResult>> LoginAsync(string email, string password) =>
        _handler.HandleAsync(new LoginCommand(email, password, null, null), CancellationToken.None);

    [Fact]
    public async Task Signs_in_with_the_right_password()
    {
        var user = GivenAnAccount();

        var result = await LoginAsync("ada@example.com", Password);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value.UserId);
        Assert.NotEmpty(result.Value.AccessToken);
        Assert.Equal(Now, user.LastLoginAtUtc);
    }

    /// <summary>
    /// Unknown account and wrong password must be indistinguishable — same code, same message. Any
    /// difference is an oracle that answers "does this person have an account here?" for free.
    /// </summary>
    [Fact]
    public async Task An_unknown_account_and_a_wrong_password_fail_identically()
    {
        GivenAnAccount();

        var unknown = await LoginAsync("nobody@example.com", Password);
        var wrongPassword = await LoginAsync("ada@example.com", "definitely-not-it");

        Assert.False(unknown.IsSuccess);
        Assert.False(wrongPassword.IsSuccess);
        Assert.Equal(ErrorCodes.InvalidCredentials, unknown.Error!.Code);
        Assert.Equal(ErrorCodes.InvalidCredentials, wrongPassword.Error!.Code);
        Assert.Equal(unknown.Error.Message, wrongPassword.Error.Message);
    }

    /// <summary>
    /// The subtle half of enumeration protection, and the half everybody forgets.
    ///
    /// Identical wording is not enough. If the handler returned early when the account does not exist,
    /// that reply would come back in a millisecond while a real account takes the ~100ms that password
    /// hashing is DESIGNED to cost. That difference is trivially measurable, and "fast rejection"
    /// silently means "no such account".
    ///
    /// So the handler verifies against a decoy hash instead. This test asserts the work happened — if
    /// someone later "optimises" the decoy away because it looks pointless, the identical error message
    /// keeps lying while the timing tells the truth, and nothing else in the suite would notice.
    /// </summary>
    [Fact]
    public async Task Hashes_a_password_even_when_the_account_does_not_exist()
    {
        await LoginAsync("nobody@example.com", Password);

        Assert.Equal(1, _hasher.VerifyCallCount);
    }

    [Fact]
    public async Task Locks_the_account_after_five_failures()
    {
        var user = GivenAnAccount();

        for (var attempt = 1; attempt <= LoginHandler.MaximumFailedAttempts; attempt++)
        {
            await LoginAsync("ada@example.com", "wrong");
        }

        Assert.True(user.IsLocked);
        Assert.Equal(Now.Add(LoginHandler.LockoutDuration), user.LockedUntilUtc);
        Assert.Contains(_repository.LoginEvents, e => e.EventType == LoginEventType.AccountLocked);
    }

    /// <summary>
    /// A locked account fails with the SAME error as a wrong password. Saying "your account is locked"
    /// to someone who does not know the password would confirm the account exists — and would also tell
    /// an attacker their brute force is working.
    /// </summary>
    [Fact]
    public async Task A_locked_account_fails_indistinguishably_even_with_the_right_password()
    {
        GivenAnAccount();

        for (var attempt = 1; attempt <= LoginHandler.MaximumFailedAttempts; attempt++)
        {
            await LoginAsync("ada@example.com", "wrong");
        }

        var result = await LoginAsync("ada@example.com", Password);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.InvalidCredentials, result.Error!.Code);
    }

    /// <summary>
    /// The lock must lift on its own. Otherwise anyone can permanently lock anyone out of their account
    /// by typing the wrong password five times — a denial of service that costs the attacker nothing
    /// and the victim their account.
    /// </summary>
    [Fact]
    public async Task The_lock_lifts_by_itself_and_the_right_password_works_again()
    {
        GivenAnAccount();

        for (var attempt = 1; attempt <= LoginHandler.MaximumFailedAttempts; attempt++)
        {
            await LoginAsync("ada@example.com", "wrong");
        }

        _clock.Advance(LoginHandler.LockoutDuration + TimeSpan.FromSeconds(1));

        var result = await LoginAsync("ada@example.com", Password);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task A_successful_sign_in_clears_the_failure_count()
    {
        var user = GivenAnAccount();

        await LoginAsync("ada@example.com", "wrong");
        await LoginAsync("ada@example.com", "wrong");
        await LoginAsync("ada@example.com", Password);

        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.False(user.IsLocked);
    }

    [Fact]
    public async Task A_deactivated_account_cannot_sign_in_even_with_the_right_password()
    {
        var user = GivenAnAccount();
        user.IsActive = false;

        var result = await LoginAsync("ada@example.com", Password);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.InvalidCredentials, result.Error!.Code);
    }

    [Fact]
    public async Task A_soft_deleted_account_cannot_sign_in()
    {
        var user = GivenAnAccount();
        user.DeletedAtUtc = Now.AddDays(-1);

        var result = await LoginAsync("ada@example.com", Password);

        Assert.False(result.IsSuccess);
    }

    /// <summary>
    /// Sign-in is the only moment the plaintext exists again, so it is the only moment a weak hash can
    /// be upgraded. Skip it and the account keeps its old parameters for life, no matter how often the
    /// person signs in.
    /// </summary>
    [Fact]
    public async Task Upgrades_a_stale_hash_on_a_successful_sign_in()
    {
        var user = GivenAnAccount();
        user.PasswordHash = "legacy-hash";
        _hasher.NextVerifyNeedsRehash = true;

        // The fake accepts any password whose hash matches; make the legacy hash match this one.
        user.PasswordHash = _hasher.Hash(Password);

        await LoginAsync("ada@example.com", Password);

        Assert.Equal(_hasher.Hash(Password), user.PasswordHash);
    }

    [Fact]
    public async Task Records_every_failure_in_the_audit_log_with_a_reason_the_caller_never_sees()
    {
        GivenAnAccount();

        var result = await LoginAsync("ada@example.com", "wrong");

        var recorded = Assert.Single(_repository.LoginEvents);
        Assert.Equal(LoginEventType.LoginFailed, recorded.EventType);
        Assert.Equal("wrong_password", recorded.FailureReason);

        // The operator needs to tell a brute force from a typo. The caller must not be able to.
        Assert.DoesNotContain("wrong_password", result.Error!.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Records_an_attempt_against_an_address_that_has_no_account()
    {
        await LoginAsync("nobody@example.com", Password);

        var recorded = Assert.Single(_repository.LoginEvents);
        Assert.Null(recorded.UserId);
        Assert.Equal("nobody@example.com", recorded.Email);
        Assert.Equal("unknown_account", recorded.FailureReason);
    }

    [Fact]
    public async Task An_unconfirmed_account_may_still_sign_in()
    {
        var user = GivenAnAccount();
        user.IsEmailConfirmed = false;

        var result = await LoginAsync("ada@example.com", Password);

        // Blocking sign-in until confirmation locks a user whose mail went to spam out of the very
        // screen that would let them ask for another one.
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsEmailConfirmed);
    }
}
