using WhyStack.Application.Identity.Logout;
using WhyStack.Application.Identity.Refresh;
using WhyStack.Application.Identity.Sessions;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Tests.Identity;

public class LogoutHandlerTests
{
    private static readonly DateTime Now = new(2026, 7, 12, 12, 0, 0, DateTimeKind.Utc);
    private static readonly SessionContext Context = new("Web", null, null, null);

    private readonly FakeIdentityRepository _repository = new();
    private readonly FakeClock _clock = new(Now);
    private readonly SessionService _sessions;
    private readonly LogoutHandler _handler;
    private readonly RefreshHandler _refresh;
    private readonly User _user;

    public LogoutHandlerTests()
    {
        _sessions = new SessionService(_repository, new FakeTokenGenerator(), new FakeTokenHasher(), _clock);
        _handler = new LogoutHandler(_repository, _sessions, _clock);
        _refresh = new RefreshHandler(_repository, _sessions, new FakeAccessTokenIssuer(), _clock);

        _user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "ada@example.com",
            NormalizedEmail = "ADA@EXAMPLE.COM",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAtUtc = Now,
        };

        _repository.Users.Add(_user);
    }

    private string GivenASignIn() => _sessions.StartSession(_user.Id, Context).RawRefreshToken;

    private Task LogoutAsync(string? token, bool allDevices = false) =>
        _handler.HandleAsync(new LogoutCommand(token, allDevices, Context), CancellationToken.None);

    [Fact]
    public async Task Ends_the_session_and_the_token_stops_working()
    {
        var token = GivenASignIn();

        await LogoutAsync(token);

        var afterwards = await _refresh.HandleAsync(new RefreshCommand(token, Context), CancellationToken.None);
        Assert.False(afterwards.IsSuccess);
    }

    /// <summary>
    /// Logging out kills the whole family, not just the token in hand. The family IS the sign-in on
    /// this device — the chain of rotations descended from it — and leaving an ancestor alive would
    /// leave a token behind that still refreshes.
    /// </summary>
    [Fact]
    public async Task Ends_every_token_in_the_family_including_the_ones_it_was_rotated_from()
    {
        var first = GivenASignIn();
        var refreshed = await _refresh.HandleAsync(new RefreshCommand(first, Context), CancellationToken.None);
        var current = refreshed.Value.RefreshToken;

        await LogoutAsync(current);

        Assert.All(_repository.Sessions, session => Assert.NotNull(session.RevokedAtUtc));
        Assert.All(_repository.Sessions, session =>
            Assert.Equal(SessionRevocationReason.Logout, session.RevocationReason));
    }

    [Fact]
    public async Task Logging_out_of_one_device_leaves_the_others_signed_in()
    {
        var phone = GivenASignIn();
        var laptop = GivenASignIn();

        await LogoutAsync(phone);

        var laptopStillWorks = await _refresh.HandleAsync(new RefreshCommand(laptop, Context), CancellationToken.None);
        Assert.True(laptopStillWorks.IsSuccess);
    }

    [Fact]
    public async Task Logging_out_of_all_devices_ends_every_session()
    {
        var phone = GivenASignIn();
        var laptop = GivenASignIn();

        await LogoutAsync(phone, allDevices: true);

        var laptopIsDead = await _refresh.HandleAsync(new RefreshCommand(laptop, Context), CancellationToken.None);
        Assert.False(laptopIsDead.IsSuccess);
    }

    /// <summary>
    /// Logout is the one operation where failure must never be visible. A client told "logout failed"
    /// has no useful response — it cannot un-know the token, and the user reasonably concludes they are
    /// still signed in. The state they asked for is true in every one of these cases.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a-token-that-never-existed")]
    public async Task Succeeds_even_when_there_is_nothing_to_end(string? token)
    {
        var result = await _handler.HandleAsync(
            new LogoutCommand(token, AllDevices: false, Context),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.SessionsEnded);
    }

    [Fact]
    public async Task Logging_out_twice_is_not_an_error()
    {
        var token = GivenASignIn();

        await LogoutAsync(token);
        var second = await _handler.HandleAsync(
            new LogoutCommand(token, AllDevices: false, Context),
            CancellationToken.None);

        Assert.True(second.IsSuccess);
    }

    /// <summary>
    /// A revoked token presented again is NOT reuse. It is a client that has not noticed yet — a phone
    /// coming back from aeroplane mode, a tab restored from yesterday. Treating it as a replay would
    /// mean "logged out" and "compromised" produce the same audit event, and the one that matters would
    /// drown in the one that does not.
    /// </summary>
    [Fact]
    public async Task Refreshing_after_logout_is_not_reported_as_a_reuse_attack()
    {
        var token = GivenASignIn();
        await LogoutAsync(token);

        await _refresh.HandleAsync(new RefreshCommand(token, Context), CancellationToken.None);

        Assert.DoesNotContain(_repository.LoginEvents, e => e.EventType == LoginEventType.TokenReuseDetected);
    }
}
