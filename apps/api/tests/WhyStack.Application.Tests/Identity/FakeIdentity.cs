using WhyStack.Application.Abstractions;
using WhyStack.Domain.Identity;

namespace WhyStack.Application.Tests.Identity;

// Hand-written fakes rather than a mocking library. Not dogma: these are five small classes, and a
// fake that RECORDS what happened reads better in an assertion than a mock that verifies it did —
// `Assert.Single(emails)` says more than `Verify(x => x.Send(It.IsAny<...>()), Times.Once)`.

public sealed class FakeClock(DateTime now) : IClock
{
    public DateTime UtcNow { get; set; } = now;

    public void Advance(TimeSpan by) => UtcNow = UtcNow.Add(by);
}

/// <summary>
/// A hasher with no cryptography, because the tests are about the LOGIC around hashing, not about
/// PBKDF2 — which is Microsoft's code, already tested, and not ours to re-verify (ADR-0017).
///
/// It counts verifications, which is what lets the timing-oracle test assert the thing that actually
/// matters: that a login against an unknown account still does the hashing work.
/// </summary>
public sealed class FakePasswordHasher : IPasswordHasher
{
    public int VerifyCallCount { get; private set; }

    public bool NextVerifyNeedsRehash { get; set; }

    // Base64, not the plaintext with a prefix. The first version of this fake was $"hashed:{password}",
    // and Never_stores_the_password caught it immediately — a fake that leaks the input is a fake that
    // would let the real thing leak it too and still go green.
    public string Hash(string password) =>
        "hashed:" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));

    public PasswordVerificationResult Verify(string hash, string password)
    {
        VerifyCallCount++;

        if (hash != Hash(password))
        {
            return PasswordVerificationResult.Failed;
        }

        return NextVerifyNeedsRehash
            ? PasswordVerificationResult.SuccessRehashNeeded
            : PasswordVerificationResult.Success;
    }
}

public sealed class FakeAccessTokenIssuer : IAccessTokenIssuer
{
    public AccessToken Issue(User user, IReadOnlyCollection<RoleName> roles) =>
        new($"token-for-{user.Id}", DateTime.UtcNow.AddMinutes(15));
}

public sealed class FakeEmailSender : IEmailSender
{
    public List<EmailMessage> Sent { get; } = [];

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        Sent.Add(message);
        return Task.CompletedTask;
    }
}

/// <summary>
/// A token generator that is predictable ON PURPOSE, so a test can name the token it expects.
/// The real one is 256 bits of CSPRNG output — see CryptoRandomTokenGenerator, and the remarks there
/// on why Guid.NewGuid() and Random are both wrong.
/// </summary>
public sealed class FakeTokenGenerator : ITokenGenerator
{
    private int _next;

    public string NewToken() => $"refresh-token-{++_next}";
}

/// <summary>
/// Deterministic, and it does NOT contain its input.
///
/// The first version returned $"sha256:{value}". That is the same mistake FakePasswordHasher made, and
/// it would have made The_raw_token_is_never_stored pass while proving nothing — a fake that embeds the
/// secret cannot catch code that stores the secret.
/// </summary>
public sealed class FakeTokenHasher : ITokenHasher
{
    public string Hash(string value) =>
        "sha256:" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(value)));
}

public sealed class FakeIdentityRepository : IIdentityRepository
{
    public List<User> Users { get; } = [];
    public List<UserRole> UserRoles { get; } = [];
    public List<UserLoginEvent> LoginEvents { get; } = [];
    public List<UserSession> Sessions { get; } = [];
    public int SaveCount { get; private set; }

    private static readonly Dictionary<RoleName, Guid> RoleIds =
        Enum.GetValues<RoleName>().ToDictionary(role => role, role => Guid.NewGuid());

    public Task<User?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        Task.FromResult(Users.SingleOrDefault(user =>
            user.NormalizedEmail == normalizedEmail && user.DeletedAtUtc is null));

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        Task.FromResult(Users.Any(user =>
            user.NormalizedEmail == normalizedEmail && user.DeletedAtUtc is null));

    public Task<IReadOnlyCollection<RoleName>> GetRolesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var roles = UserRoles
            .Where(userRole => userRole.UserId == userId)
            .Select(userRole => RoleIds.First(pair => pair.Value == userRole.RoleId).Key)
            .ToList();

        return Task.FromResult<IReadOnlyCollection<RoleName>>(roles);
    }

    public Task<Guid> GetRoleIdAsync(RoleName role, CancellationToken cancellationToken) =>
        Task.FromResult(RoleIds[role]);

    public Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(Users.SingleOrDefault(user => user.Id == userId));

    public void AddUser(User user) => Users.Add(user);

    public void AddUserRole(UserRole userRole) => UserRoles.Add(userRole);

    public void AddLoginEvent(UserLoginEvent loginEvent) => LoginEvents.Add(loginEvent);

    public void AddSession(UserSession session) => Sessions.Add(session);

    /// <summary>
    /// Finds revoked and rotated sessions too — exactly like the real one, and for the same reason: a
    /// lookup that filtered to usable sessions would make a replayed token simply "not found", and
    /// reuse detection would vanish without a single test noticing.
    /// </summary>
    public Task<UserSession?> FindSessionByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        Task.FromResult(Sessions.SingleOrDefault(session => session.RefreshTokenHash == tokenHash));

    public Task<IReadOnlyCollection<UserSession>> GetFamilyAsync(Guid familyId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyCollection<UserSession>>(
            Sessions.Where(session => session.FamilyId == familyId).ToList());

    public Task<IReadOnlyCollection<UserSession>> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyCollection<UserSession>>(
            Sessions.Where(session => session.UserId == userId && session.RevokedAtUtc is null).ToList());

    public List<EmailConfirmationToken> EmailConfirmationTokens { get; } = [];
    public List<PasswordResetToken> PasswordResetTokens { get; } = [];

    public void AddEmailConfirmationToken(EmailConfirmationToken token) => EmailConfirmationTokens.Add(token);

    public Task<EmailConfirmationToken?> FindEmailConfirmationTokenAsync(string tokenHash, CancellationToken cancellationToken) =>
        Task.FromResult(EmailConfirmationTokens.SingleOrDefault(token => token.TokenHash == tokenHash));

    public void AddPasswordResetToken(PasswordResetToken token) => PasswordResetTokens.Add(token);

    public Task<PasswordResetToken?> FindPasswordResetTokenAsync(string tokenHash, CancellationToken cancellationToken) =>
        Task.FromResult(PasswordResetTokens.SingleOrDefault(token => token.TokenHash == tokenHash));

    public Task InvalidateOutstandingPasswordResetTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        foreach (var token in PasswordResetTokens.Where(t => t.UserId == userId && t.UsedAtUtc is null))
        {
            token.UsedAtUtc = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }

    public Task InvalidateOutstandingEmailConfirmationTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        foreach (var token in EmailConfirmationTokens.Where(t => t.UserId == userId && t.UsedAtUtc is null))
        {
            token.UsedAtUtc = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}

/// <summary>Links that are obviously fake and obviously links, so a test can assert on either.</summary>
public sealed class FakeAppLinks : IAppLinks
{
    public string ConfirmEmail(string token) => $"https://test.local/auth/confirm-email?token={token}";

    public string ResetPassword(string token) => $"https://test.local/auth/reset-password?token={token}";
}
