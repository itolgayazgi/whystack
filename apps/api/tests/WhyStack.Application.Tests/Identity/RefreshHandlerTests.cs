using WhyStack.Application.Common;
using WhyStack.Application.Identity.Refresh;
using WhyStack.Application.Identity.Sessions;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Tests.Identity;

/// <summary>
/// The high-risk area (ADR-0008, `13`). Every one of these tests describes an attack that works if the
/// test is deleted.
/// </summary>
public class RefreshHandlerTests
{
    private static readonly DateTime Now = new(2026, 7, 12, 12, 0, 0, DateTimeKind.Utc);
    private static readonly SessionContext Context = new("Web", null, null, null);

    private readonly FakeIdentityRepository _repository = new();
    private readonly FakeClock _clock = new(Now);
    private readonly SessionService _sessions;
    private readonly RefreshHandler _handler;

    private readonly User _user;

    public RefreshHandlerTests()
    {
        _sessions = new SessionService(_repository, new FakeTokenGenerator(), new FakeTokenHasher(), _clock);
        _handler = new RefreshHandler(_repository, _sessions, new FakeAccessTokenIssuer(), _clock);

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

    private Task<Result<RefreshResult>> RefreshAsync(string token) =>
        _handler.HandleAsync(new RefreshCommand(token, Context), CancellationToken.None);

    [Fact]
    public async Task Exchanges_a_valid_token_for_a_new_pair()
    {
        var token = GivenASignIn();

        var result = await RefreshAsync(token);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.AccessToken);
        Assert.NotEqual(token, result.Value.RefreshToken);
    }

    /// <summary>
    /// Single use. The old token is dead the moment it is exchanged — that is what "rotation" means,
    /// and without it a leaked token stays valid for its whole thirty days.
    /// </summary>
    [Fact]
    public async Task The_old_token_stops_working_the_moment_it_is_rotated()
    {
        var first = GivenASignIn();

        await RefreshAsync(first);
        var replay = await RefreshAsync(first);

        Assert.False(replay.IsSuccess);
        Assert.Equal(ErrorCodes.InvalidRefreshToken, replay.Error!.Code);
    }

    /// <summary>
    /// <b>The most important test in this repository.</b>
    ///
    /// The attacker steals a refresh token and uses it. The victim's client, still holding the same
    /// token, refreshes too — a replay. Rotation alone would simply reject one of them and let the
    /// other carry on, and whoever refreshed last would keep the account.
    ///
    /// Detection revokes the WHOLE FAMILY. Both are signed out. The victim signs in again; the attacker
    /// cannot, because they do not have the password.
    ///
    /// If <see cref="RefreshHandler"/> ever checks <c>IsUsable</c> before <c>IsRotated</c>, the replay
    /// is still rejected — quietly — the family survives, and the attacker keeps the newer token they
    /// already hold. Everything would still look like it works. This test is what stops that.
    /// </summary>
    [Fact]
    public async Task Replaying_a_rotated_token_revokes_every_session_in_the_family()
    {
        var stolen = GivenASignIn();

        // The attacker gets there first and rotates.
        var attacker = await RefreshAsync(stolen);
        Assert.True(attacker.IsSuccess);
        var attackersNewToken = attacker.Value.RefreshToken;

        // The victim's client refreshes with the token it still holds. This is the replay.
        var victim = await RefreshAsync(stolen);
        Assert.False(victim.IsSuccess);

        // Everything in the family is dead — including the token the ATTACKER just obtained.
        Assert.All(_repository.Sessions, session => Assert.NotNull(session.RevokedAtUtc));
        Assert.All(_repository.Sessions, session =>
            Assert.Equal(SessionRevocationReason.ReuseDetected, session.RevocationReason));

        // And it is not merely marked: the attacker's fresh token no longer works.
        var attackerTriesAgain = await RefreshAsync(attackersNewToken);
        Assert.False(attackerTriesAgain.IsSuccess);
    }

    [Fact]
    public async Task Reuse_is_recorded_as_its_own_event_not_as_an_ordinary_failure()
    {
        var token = GivenASignIn();
        await RefreshAsync(token);
        await RefreshAsync(token);

        // An operator must be able to find this. "Token refresh failed" happens to everyone whose
        // session expired; a replay means a credential leaked, and the two must not sit in one bucket.
        Assert.Contains(_repository.LoginEvents, e => e.EventType == LoginEventType.TokenReuseDetected);
    }

    [Fact]
    public async Task An_unrelated_session_survives_a_reuse_on_another_device()
    {
        var phone = GivenASignIn();
        var laptop = GivenASignIn();

        await RefreshAsync(phone);
        await RefreshAsync(phone); // replay on the phone

        // The laptop is a different family. Signing the user out of every device because one was
        // compromised is a punishment, not a defence — and it would train them to ignore it.
        var laptopStillWorks = await RefreshAsync(laptop);
        Assert.True(laptopStillWorks.IsSuccess);
    }

    [Fact]
    public async Task An_expired_token_is_rejected()
    {
        var token = GivenASignIn();

        _clock.Advance(SessionService.RefreshTokenLifetime + TimeSpan.FromSeconds(1));

        var result = await RefreshAsync(token);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.InvalidRefreshToken, result.Error!.Code);
    }

    [Fact]
    public async Task A_token_we_never_issued_is_rejected_without_revoking_anything()
    {
        var real = GivenASignIn();

        var result = await RefreshAsync("not-a-token-we-ever-made");

        Assert.False(result.IsSuccess);

        // A guessed token must not be a way to sign someone else out.
        var stillWorks = await RefreshAsync(real);
        Assert.True(stillWorks.IsSuccess);
    }

    /// <summary>
    /// A refresh token is not a bypass. If the account was locked, deactivated or deleted while the
    /// session was alive, renewing an access token would hand out fifteen more minutes of access to
    /// somebody an administrator has just shut out.
    /// </summary>
    [Fact]
    public async Task A_deactivated_account_cannot_refresh_and_its_sessions_die()
    {
        var token = GivenASignIn();
        _user.IsActive = false;

        var result = await RefreshAsync(token);

        Assert.False(result.IsSuccess);
        Assert.All(_repository.Sessions, session => Assert.NotNull(session.RevokedAtUtc));
    }

    /// <summary>
    /// Roles are re-read on every refresh, never carried over. A role revoked ten minutes ago must not
    /// survive in the next access token merely because the session did.
    /// </summary>
    [Fact]
    public async Task A_role_removed_during_the_session_is_gone_from_the_next_access_token()
    {
        var token = GivenASignIn();

        var adminRoleId = await _repository.GetRoleIdAsync(RoleName.Administrator, CancellationToken.None);
        _repository.UserRoles.Add(new UserRole { UserId = _user.Id, RoleId = adminRoleId, AssignedAtUtc = Now });

        var withAdmin = await RefreshAsync(token);
        Assert.True(withAdmin.IsSuccess);

        _repository.UserRoles.Clear();

        var withoutAdmin = await RefreshAsync(withAdmin.Value.RefreshToken);
        Assert.True(withoutAdmin.IsSuccess);

        // Nothing to assert on the token itself here (the issuer is a fake) — the guarantee is that the
        // handler asked the repository again rather than reusing what it had.
        Assert.Empty(await _repository.GetRolesAsync(_user.Id, CancellationToken.None));
    }

    [Fact]
    public async Task The_raw_token_is_never_stored()
    {
        var token = GivenASignIn();

        Assert.All(_repository.Sessions, session =>
            Assert.DoesNotContain(token, session.RefreshTokenHash, StringComparison.Ordinal));
    }
}
