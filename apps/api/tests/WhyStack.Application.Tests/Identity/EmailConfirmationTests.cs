using WhyStack.Application.Common;
using WhyStack.Application.Identity.Confirmation;
using WhyStack.Application.Identity.Register;
using WhyStack.Application.Identity.Tokens;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Tests.Identity;

public class EmailConfirmationTests
{
    private static readonly DateTime Now = new(2026, 7, 12, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeIdentityRepository _repository = new();
    private readonly FakeEmailSender _email = new();
    private readonly FakeClock _clock = new(Now);

    private readonly SingleUseTokenService _tokens;
    private readonly RegisterUserHandler _register;
    private readonly ConfirmEmailHandler _confirm;
    private readonly ResendConfirmationHandler _resend;

    public EmailConfirmationTests()
    {
        _tokens = new SingleUseTokenService(_repository, new FakeTokenGenerator(), new FakeTokenHasher(), _clock);
        _register = new RegisterUserHandler(
            _repository, new FakePasswordHasher(), _tokens, _email, new FakeAppLinks(), _clock);
        _confirm = new ConfirmEmailHandler(_repository, _tokens, _clock);
        _resend = new ResendConfirmationHandler(_repository, _tokens, _email, new FakeAppLinks());
    }

    private Task RegisterAsync(string email = "ada@example.com") =>
        _register.HandleAsync(
            new RegisterUserCommand(email, "a-good-long-password", null, null, null),
            CancellationToken.None);

    private string TokenFromEmail() =>
        _email.Sent[^1].Body.Split("token=")[1].Split('\n')[0].Trim();

    private Task<Result<ConfirmEmailResult>> ConfirmAsync(string? token) =>
        _confirm.HandleAsync(new ConfirmEmailCommand(token, null, null), CancellationToken.None);

    [Fact]
    public async Task Registration_sends_a_confirmation_link()
    {
        await RegisterAsync();

        var mail = Assert.Single(_email.Sent);
        Assert.Contains("/auth/confirm-email?token=", mail.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_new_account_is_not_confirmed()
    {
        await RegisterAsync();

        Assert.False(_repository.Users[0].IsEmailConfirmed);
    }

    [Fact]
    public async Task Clicking_the_link_confirms_the_address()
    {
        await RegisterAsync();

        var result = await ConfirmAsync(TokenFromEmail());

        Assert.True(result.IsSuccess);
        Assert.True(_repository.Users[0].IsEmailConfirmed);
        Assert.Contains(_repository.LoginEvents, e => e.EventType == LoginEventType.EmailConfirmed);
    }

    [Fact]
    public async Task The_link_works_exactly_once()
    {
        await RegisterAsync();
        var token = TokenFromEmail();

        await ConfirmAsync(token);
        var second = await ConfirmAsync(token);

        Assert.False(second.IsSuccess);
        Assert.Equal(ErrorCodes.InvalidConfirmationToken, second.Error!.Code);
    }

    [Fact]
    public async Task An_expired_link_is_refused()
    {
        await RegisterAsync();
        var token = TokenFromEmail();

        _clock.Advance(SingleUseTokenService.EmailConfirmationLifetime + TimeSpan.FromSeconds(1));

        var result = await ConfirmAsync(token);

        Assert.False(result.IsSuccess);
    }

    /// <summary>
    /// The attack this guards against:
    ///
    /// Register as <c>ada@attacker.com</c>. Get the confirmation link. Change the account's address to
    /// <c>victim@bank.com</c>. Now click the link.
    ///
    /// Without this check, the account ends up with a "confirmed" address that nobody at that address
    /// ever agreed to — and every downstream feature that trusts <c>IsEmailConfirmed</c> is trusting a
    /// lie. The token confirms the address it was SENT to, not whatever the row says by the time it is
    /// redeemed.
    /// </summary>
    [Fact]
    public async Task A_token_cannot_confirm_an_address_it_was_not_sent_to()
    {
        await RegisterAsync("ada@example.com");
        var token = TokenFromEmail();

        _repository.Users[0].Email = "victim@bank.com";

        var result = await ConfirmAsync(token);

        Assert.False(result.IsSuccess);
        Assert.False(_repository.Users[0].IsEmailConfirmed);
    }

    [Fact]
    public async Task A_token_nobody_issued_is_refused()
    {
        var result = await ConfirmAsync("not-a-real-token");

        Assert.False(result.IsSuccess);
    }

    // ── Resend ────────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Resending_issues_a_new_link_and_kills_the_old_one()
    {
        await RegisterAsync();
        var first = TokenFromEmail();

        await _resend.HandleAsync(new ResendConfirmationCommand("ada@example.com"), CancellationToken.None);
        var second = TokenFromEmail();

        Assert.NotEqual(first, second);

        Assert.False((await ConfirmAsync(first)).IsSuccess);
        Assert.True((await ConfirmAsync(second)).IsSuccess);
    }

    /// <summary>
    /// "That address is already confirmed" is a perfectly friendly way to tell an attacker both that
    /// the account exists AND that it is in active use.
    /// </summary>
    [Fact]
    public async Task Resending_answers_identically_for_unknown_and_already_confirmed_addresses()
    {
        await RegisterAsync("ada@example.com");
        await ConfirmAsync(TokenFromEmail());
        _email.Sent.Clear();

        var alreadyConfirmed = await _resend.HandleAsync(
            new ResendConfirmationCommand("ada@example.com"), CancellationToken.None);

        var unknown = await _resend.HandleAsync(
            new ResendConfirmationCommand("nobody@example.com"), CancellationToken.None);

        Assert.True(alreadyConfirmed.IsSuccess);
        Assert.True(unknown.IsSuccess);
        Assert.Equal(alreadyConfirmed.Value.Message, unknown.Value.Message);

        // And neither sent anything — the observable difference exists only in an inbox we do not own.
        Assert.Empty(_email.Sent);
    }
}
