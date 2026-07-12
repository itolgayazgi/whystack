using WhyStack.Application.Abstractions;
using WhyStack.Domain.Identity;
using WhyStack.Domain.Users;

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
    public List<UserPreferences> Preferences { get; } = [];
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

    public void AddPreferences(UserPreferences preferences) => Preferences.Add(preferences);

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

/// <summary>
/// Preferences, plus a rowversion that behaves like SQL Server's: it CHANGES ON EVERY WRITE.
/// </summary>
/// <remarks>
/// That last part is the whole point of this fake. A stub that simply saved and returned true would let
/// every optimistic-concurrency test pass without optimistic concurrency existing at all — the conflict
/// would never be detected because nothing would ever change the version. Incrementing it here is what
/// makes "the second writer is rejected" a claim the test can actually falsify.
/// </remarks>
public sealed class FakeUserPreferencesRepository : IUserPreferencesRepository
{
    private readonly List<UserPreferences> _stored = [];

    public int SaveCount { get; private set; }

    /// <summary>What the "database" holds — never what an in-flight, unsaved handler is holding.</summary>
    public IReadOnlyList<UserPreferences> Preferences => _stored;

    /// <summary>
    /// Returns a COPY, and that detail is the whole reason this fake is trustworthy.
    /// </summary>
    /// <remarks>
    /// The first version handed back the stored reference. The handler then mutated it BEFORE calling
    /// TrySaveAsync — as it must; that is how EF Core change tracking works — and the mutation was
    /// instantly visible in the fake's list even when the save was REJECTED. So a rejected write still
    /// appeared to have overwritten the row, and the conflict test failed while the real code was right.
    ///
    /// A real database does not work that way: an UPDATE that matches zero rows changes nothing, and the
    /// entity the failed request was holding dies with its DbContext. Copying on read and committing
    /// only on a successful save is what makes this fake model that, instead of modelling a shared
    /// mutable list that happens to be called a repository.
    /// </remarks>
    public Task<UserPreferences?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(Copy(_stored.SingleOrDefault(row => row.UserId == userId)));

    public Task<bool> TrySaveAsync(
        UserPreferences preferences,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken)
    {
        var stored = _stored.SingleOrDefault(row => row.Id == preferences.Id);

        if (stored is null)
        {
            return Task.FromResult(false);
        }

        // Exactly what SQL Server does: the UPDATE matches zero rows once the version has moved on, and
        // nothing is written.
        if (!(stored.RowVersion ?? []).SequenceEqual(expectedRowVersion))
        {
            return Task.FromResult(false);
        }

        stored.ApplicationLanguageCode = preferences.ApplicationLanguageCode;
        stored.ContentLanguageCode = preferences.ContentLanguageCode;
        stored.ThemeMode = preferences.ThemeMode;
        stored.ReadingFontScale = preferences.ReadingFontScale;
        stored.ReducedMotionEnabled = preferences.ReducedMotionEnabled;
        stored.PreferredSkillLevel = preferences.PreferredSkillLevel;
        stored.UpdatedAtUtc = preferences.UpdatedAtUtc;
        stored.RowVersion = NextVersion(stored.RowVersion);

        // The caller's entity gets the new version too — EF Core does this after a successful save, and
        // the handler returns it to the client as the token for the NEXT write. Skip it and every second
        // write from a well-behaved client would 409.
        preferences.RowVersion = stored.RowVersion;

        SaveCount++;

        return Task.FromResult(true);
    }

    public UserPreferences Seed(Guid userId, DateTime createdAtUtc)
    {
        var preferences = new UserPreferences
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            ApplicationLanguageCode = "en",
            ContentLanguageCode = "en",
            ThemeMode = ThemeMode.System,
            ReadingFontScale = 1.0,
            ReducedMotionEnabled = false,
            CreatedAtUtc = createdAtUtc,
            RowVersion = NextVersion(null),
        };

        _stored.Add(preferences);

        return Copy(preferences)!;
    }

    private static UserPreferences? Copy(UserPreferences? source) =>
        source is null
            ? null
            : new UserPreferences
            {
                Id = source.Id,
                UserId = source.UserId,
                ApplicationLanguageCode = source.ApplicationLanguageCode,
                ContentLanguageCode = source.ContentLanguageCode,
                ThemeMode = source.ThemeMode,
                ReadingFontScale = source.ReadingFontScale,
                ReducedMotionEnabled = source.ReducedMotionEnabled,
                PreferredSkillLevel = source.PreferredSkillLevel,
                CreatedAtUtc = source.CreatedAtUtc,
                UpdatedAtUtc = source.UpdatedAtUtc,
                RowVersion = source.RowVersion?.ToArray(),
            };

    /// <summary>Eight bytes, big-endian, incremented — the shape of a SQL Server rowversion.</summary>
    private static byte[] NextVersion(byte[]? current)
    {
        var value = current is { Length: 8 }
            ? System.Buffers.Binary.BinaryPrimitives.ReadUInt64BigEndian(current)
            : 0UL;

        var next = new byte[8];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64BigEndian(next, value + 1);

        return next;
    }
}
