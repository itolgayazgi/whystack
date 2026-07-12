using WhyStack.Application.Common;
using WhyStack.Application.Identity.Register;
using WhyStack.Application.Identity.Tokens;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Tests.Identity;

public class RegisterUserHandlerTests
{
    private static readonly DateTime Now = new(2026, 7, 12, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeIdentityRepository _repository = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeEmailSender _email = new();
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        var clock = new FakeClock(Now);
        var tokens = new SingleUseTokenService(_repository, new FakeTokenGenerator(), new FakeTokenHasher(), clock);

        _handler = new RegisterUserHandler(_repository, _hasher, tokens, _email, new FakeAppLinks(), clock);
    }

    private Task<Result<RegisterUserResult>> RegisterAsync(string email, string password = "a-good-long-password") =>
        _handler.HandleAsync(new RegisterUserCommand(email, password, null, null, null), CancellationToken.None);

    [Fact]
    public async Task Registers_a_new_account_with_the_RegisteredUser_role()
    {
        var result = await RegisterAsync("ada@example.com");

        Assert.True(result.IsSuccess);

        var user = Assert.Single(_repository.Users);
        Assert.Equal("ada@example.com", user.Email);
        Assert.Equal("ADA@EXAMPLE.COM", user.NormalizedEmail);
        Assert.False(user.IsEmailConfirmed);

        Assert.Single(_repository.UserRoles);

        // The role must be assigned in the SAME save as the user. A user with no role is an account
        // that exists, can sign in, and can do nothing — and nobody would ever work out why.
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Never_stores_the_password()
    {
        await RegisterAsync("ada@example.com", "correct-horse-battery");

        var user = Assert.Single(_repository.Users);
        Assert.DoesNotContain("correct-horse-battery", user.PasswordHash, StringComparison.Ordinal);
    }

    /// <summary>
    /// The one that matters most in this file.
    ///
    /// If registering with a taken address answered differently — 409, a different message, a different
    /// status — anyone could feed this endpoint a list of email addresses and learn which of them belong
    /// to people who use the site. On a site about what someone is learning, that is worth leaking to
    /// nobody. `04` calls it account enumeration protection, and this is what it means in practice.
    /// </summary>
    [Fact]
    public async Task Answers_a_taken_address_exactly_as_it_answers_a_free_one()
    {
        var first = await RegisterAsync("ada@example.com");
        var second = await RegisterAsync("ada@example.com");

        Assert.True(second.IsSuccess);
        Assert.Equal(first.Value.Message, second.Value.Message);

        // ...and no second account was created, obviously.
        Assert.Single(_repository.Users);
    }

    /// <summary>
    /// Withholding the truth from the ATTACKER is only half of it. The person who actually owns the
    /// inbox is told exactly what happened — which is why this flow cannot be built without email.
    /// </summary>
    [Fact]
    public async Task Tells_the_inbox_owner_that_someone_tried_to_register_with_their_address()
    {
        await RegisterAsync("ada@example.com");
        _email.Sent.Clear();

        await RegisterAsync("ada@example.com");

        var mail = Assert.Single(_email.Sent);
        Assert.Equal("ada@example.com", mail.To);
        Assert.Contains("already have", mail.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Treats_the_same_address_in_a_different_case_as_the_same_address()
    {
        await RegisterAsync("ada@example.com");
        await RegisterAsync("ADA@Example.COM");

        Assert.Single(_repository.Users);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@example.com")]
    [InlineData("ada@")]
    [InlineData("ada @example.com")]
    [InlineData("")]
    public async Task Rejects_an_address_that_is_not_an_address(string email)
    {
        var result = await RegisterAsync(email);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ValidationFailed, result.Error!.Code);
        Assert.Contains("email", result.Error.FieldErrors!.Keys);
    }

    [Fact]
    public async Task Rejects_a_password_that_is_too_short()
    {
        var result = await RegisterAsync("ada@example.com", "short");

        Assert.False(result.IsSuccess);
        Assert.Contains("password", result.Error!.FieldErrors!.Keys);
    }

    /// <summary>
    /// Not a strength rule — a hashing-cost rule. Hashing is expensive on purpose, so an unbounded
    /// password is a free way to make the server burn CPU on every attempt, and enough of them at once
    /// is a denial of service that costs the attacker nothing.
    /// </summary>
    [Fact]
    public async Task Rejects_a_password_long_enough_to_be_a_denial_of_service()
    {
        var result = await RegisterAsync("ada@example.com", new string('x', 100_000));

        Assert.False(result.IsSuccess);
        Assert.Contains("password", result.Error!.FieldErrors!.Keys);
    }

    [Fact]
    public async Task Rejects_a_password_that_contains_the_email_address()
    {
        var result = await RegisterAsync("ada@example.com", "ada@example.com!");

        Assert.False(result.IsSuccess);
        Assert.Contains("password", result.Error!.FieldErrors!.Keys);
    }

    [Fact]
    public async Task Records_the_registration_in_the_audit_log()
    {
        await RegisterAsync("ada@example.com");

        var recorded = Assert.Single(_repository.LoginEvents);
        Assert.Equal(LoginEventType.RegistrationSucceeded, recorded.EventType);
        Assert.True(recorded.IsSuccessful);
    }
}
