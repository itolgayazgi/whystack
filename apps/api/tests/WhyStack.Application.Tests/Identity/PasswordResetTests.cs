using WhyStack.Application.Abstractions;
using WhyStack.Application.Common;
using WhyStack.Application.Identity.Passwords;
using WhyStack.Application.Identity.Refresh;
using WhyStack.Application.Identity.Sessions;
using WhyStack.Application.Identity.Tokens;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Tests.Identity;

public class PasswordResetTests
{
    private const string OldPassword = "the-old-password";
    private const string NewPassword = "a-brand-new-password";

    private static readonly DateTime Now = new(2026, 7, 12, 12, 0, 0, DateTimeKind.Utc);
    private static readonly SessionContext Context = new("Web", null, null, null);

    private readonly FakeIdentityRepository _repository = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeEmailSender _email = new();
    private readonly FakeClock _clock = new(Now);

    private readonly SessionService _sessions;
    private readonly SingleUseTokenService _tokens;
    private readonly ForgotPasswordHandler _forgot;
    private readonly ResetPasswordHandler _reset;
    private readonly RefreshHandler _refresh;

    private readonly User _user;

    public PasswordResetTests()
    {
        _sessions = new SessionService(_repository, new FakeTokenGenerator(), new FakeTokenHasher(), _clock);
        _tokens = new SingleUseTokenService(_repository, new FakeTokenGenerator(), new FakeTokenHasher(), _clock);
        _forgot = new ForgotPasswordHandler(_repository, _tokens, _email, new FakeAppLinks(), _clock);
        _reset = new ResetPasswordHandler(_repository, _tokens, _sessions, _hasher, _email, _clock);
        _refresh = new RefreshHandler(_repository, _sessions, new FakeAccessTokenIssuer(), _clock);

        _user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "ada@example.com",
            NormalizedEmail = "ADA@EXAMPLE.COM",
            PasswordHash = _hasher.Hash(OldPassword),
            IsActive = true,
            CreatedAtUtc = Now,
        };

        _repository.Users.Add(_user);
    }

    private Task<Result<ForgotPasswordResult>> ForgotAsync(string email) =>
        _forgot.HandleAsync(new ForgotPasswordCommand(email, null, null), CancellationToken.None);

    private Task<Result<ResetPasswordResult>> ResetAsync(string? token, string password = NewPassword) =>
        _reset.HandleAsync(new ResetPasswordCommand(token, password, null, null), CancellationToken.None);

    /// <summary>Pulls the token out of the link in the email — the same way a human does.</summary>
    private string TokenFromEmail() =>
        _email.Sent[^1].Body.Split("token=")[1].Split('\n')[0].Trim();

    // ── Enumeration ───────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// The single easiest place to lose account-enumeration protection, because the helpful answer is
    /// right there: "no account with that address". Ship that, and anyone can feed this endpoint an
    /// address list and learn who has an account here.
    /// </summary>
    [Fact]
    public async Task Forgot_password_answers_identically_for_an_unknown_address()
    {
        var known = await ForgotAsync("ada@example.com");
        var unknown = await ForgotAsync("nobody@example.com");

        Assert.True(known.IsSuccess);
        Assert.True(unknown.IsSuccess);
        Assert.Equal(known.Value.Message, unknown.Value.Message);
    }

    [Fact]
    public async Task No_email_is_sent_for_an_address_that_has_no_account()
    {
        await ForgotAsync("nobody@example.com");

        Assert.Empty(_email.Sent);
    }

    /// <summary>
    /// Forgetting your password is exactly how you get locked out. Refusing to reset a locked account
    /// would leave the user with no way back in but to wait — and would tell them, by the difference in
    /// behaviour, that they are locked.
    /// </summary>
    [Fact]
    public async Task A_locked_account_may_still_reset_its_password()
    {
        _user.IsLocked = true;
        _user.LockedUntilUtc = Now.AddMinutes(15);

        await ForgotAsync("ada@example.com");
        var result = await ResetAsync(TokenFromEmail());

        Assert.True(result.IsSuccess);
        Assert.False(_user.IsLocked);
        Assert.Equal(0, _user.FailedLoginAttempts);
    }

    // ── The token ─────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task The_raw_token_is_never_stored()
    {
        await ForgotAsync("ada@example.com");
        var raw = TokenFromEmail();

        Assert.All(_repository.PasswordResetTokens, token =>
            Assert.DoesNotContain(raw, token.TokenHash, StringComparison.Ordinal));
    }

    /// <summary>
    /// A reset link sits in an inbox — and inboxes get breached, forwarded, and synced to a laptop
    /// somebody later sells. Without single use, that link stays a working key to the account for as
    /// long as the mail exists.
    /// </summary>
    [Fact]
    public async Task A_reset_token_works_exactly_once()
    {
        await ForgotAsync("ada@example.com");
        var token = TokenFromEmail();

        var first = await ResetAsync(token);
        var second = await ResetAsync(token, "yet-another-password");

        Assert.True(first.IsSuccess);
        Assert.False(second.IsSuccess);
        Assert.Equal(ErrorCodes.InvalidResetToken, second.Error!.Code);
    }

    [Fact]
    public async Task An_expired_token_is_refused()
    {
        await ForgotAsync("ada@example.com");
        var token = TokenFromEmail();

        _clock.Advance(SingleUseTokenService.PasswordResetLifetime + TimeSpan.FromSeconds(1));

        var result = await ResetAsync(token);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.InvalidResetToken, result.Error!.Code);
    }

    /// <summary>
    /// "I clicked reset twice because the mail was slow" must not leave two live keys in an inbox. The
    /// older one — the one more likely to have been forwarded or read over a shoulder — must die.
    /// </summary>
    [Fact]
    public async Task Requesting_a_second_link_kills_the_first()
    {
        await ForgotAsync("ada@example.com");
        var first = TokenFromEmail();

        await ForgotAsync("ada@example.com");
        var second = TokenFromEmail();

        Assert.NotEqual(first, second);

        var withOldToken = await ResetAsync(first);
        Assert.False(withOldToken.IsSuccess);

        var withNewToken = await ResetAsync(second);
        Assert.True(withNewToken.IsSuccess);
    }

    [Fact]
    public async Task A_token_that_was_never_issued_is_refused()
    {
        var result = await ResetAsync("a-token-nobody-ever-made");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.InvalidResetToken, result.Error!.Code);
    }

    // ── What the reset actually does ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task The_new_password_works_and_the_old_one_does_not()
    {
        await ForgotAsync("ada@example.com");
        await ResetAsync(TokenFromEmail());

        Assert.Equal(PasswordVerificationResult.Success, _hasher.Verify(_user.PasswordHash, NewPassword));
        Assert.Equal(PasswordVerificationResult.Failed, _hasher.Verify(_user.PasswordHash, OldPassword));
    }

    /// <summary>
    /// <b>The most important assertion in this file.</b>
    ///
    /// Changing your password is what you DO when you think someone else is in your account. A reset
    /// that leaves their refresh token working means the attacker keeps the account while the victim
    /// congratulates themselves on having fixed it.
    /// </summary>
    [Fact]
    public async Task Every_session_dies_including_the_attackers()
    {
        var attackersSession = _sessions.StartSession(_user.Id, Context).RawRefreshToken;
        var victimsSession = _sessions.StartSession(_user.Id, Context).RawRefreshToken;

        await ForgotAsync("ada@example.com");
        var result = await ResetAsync(TokenFromEmail());

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.SessionsEnded);

        foreach (var token in new[] { attackersSession, victimsSession })
        {
            var stillWorks = await _refresh.HandleAsync(new RefreshCommand(token, Context), CancellationToken.None);
            Assert.False(stillWorks.IsSuccess);
        }

        Assert.All(_repository.Sessions, session =>
            Assert.Equal(SessionRevocationReason.PasswordChanged, session.RevocationReason));
    }

    /// <summary>
    /// Redeeming the link proves you read mail sent to that address — which is exactly what email
    /// confirmation proves. Leaving it unconfirmed afterwards would nag a user who has just
    /// demonstrated the thing we were nagging them about.
    /// </summary>
    [Fact]
    public async Task A_successful_reset_confirms_the_email_address()
    {
        _user.IsEmailConfirmed = false;

        await ForgotAsync("ada@example.com");
        await ResetAsync(TokenFromEmail());

        Assert.True(_user.IsEmailConfirmed);
    }

    /// <summary>
    /// If the change was not the user's doing, this email is the only way they find out — and it lands
    /// at an address the attacker does not control, which is what makes it worth sending.
    /// </summary>
    [Fact]
    public async Task The_user_is_TOLD_that_their_password_changed()
    {
        await ForgotAsync("ada@example.com");

        // Take the token first, then clear — so what is left in Sent afterwards is only the notice.
        var token = TokenFromEmail();
        _email.Sent.Clear();

        await ResetAsync(token);

        var notice = Assert.Single(_email.Sent);
        Assert.Contains("changed", notice.Subject, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("was NOT you", notice.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_weak_new_password_is_refused_and_nothing_changes()
    {
        await ForgotAsync("ada@example.com");
        var token = TokenFromEmail();
        var originalHash = _user.PasswordHash;

        var result = await ResetAsync(token, "short");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ValidationFailed, result.Error!.Code);
        Assert.Equal(originalHash, _user.PasswordHash);

        // And the token survives — a rejected password must not burn the link and force a new email.
        var retry = await ResetAsync(token);
        Assert.True(retry.IsSuccess);
    }
}
