using WhyStack.Domain.Identity;
using WhyStack.Domain.Users;

namespace WhyStack.Application.Abstractions;

// The ports the Application layer needs and does not own. Every one of them is an interface here and
// an implementation in Infrastructure — which is what keeps EF Core, ASP.NET Core and any provider SDK
// on the far side of the boundary (CLAUDE.md §3, `12`).
//
// They are also what makes the use cases testable without a database, a clock, or a web host. A test
// that has to spin up SQL Server to check that a locked account cannot sign in is a test nobody runs.

/// <summary>
/// Password hashing. Implemented by Microsoft's PasswordHasher (ADR-0017) — we do not write this.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    PasswordVerificationResult Verify(string hash, string password);
}

public enum PasswordVerificationResult
{
    Failed = 0,
    Success = 1,

    /// <summary>
    /// The password is correct, but the hash was produced with older parameters — fewer iterations, an
    /// older algorithm. Rehash it now, while we hold the plaintext, because this is the only moment we
    /// ever will. Skip this and the account keeps a weak hash forever, no matter how often the user
    /// signs in.
    /// </summary>
    SuccessRehashNeeded = 2,
}

/// <summary>Issues the short-lived JWT access token (ADR-0008: ~15 minutes).</summary>
public interface IAccessTokenIssuer
{
    AccessToken Issue(User user, IReadOnlyCollection<RoleName> roles);
}

public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);

/// <summary>
/// Time, as a dependency.
///
/// Not ceremony: half the rules in this domain are about time — a lock that expires, a token that
/// does not, a session that lasts thirty days. Testing any of them against DateTime.UtcNow means
/// either sleeping, or never testing the boundary. Both are how off-by-one bugs ship.
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}

/// <summary>
/// One-way hashing for values we must be able to compare but must not be able to read back: refresh
/// tokens, reset tokens, and the IP addresses and user agents `07` requires be stored hashed.
/// </summary>
public interface ITokenHasher
{
    string Hash(string value);
}

/// <summary>
/// Produces refresh tokens: 256 bits from a cryptographically secure RNG.
/// </summary>
/// <remarks>
/// Not <c>Guid.NewGuid()</c>, and not <c>Random</c>. A GUID is 122 bits with structure, and
/// <c>Random</c> is a predictable sequence seeded from the clock — an attacker who observes one token
/// can generate the next. This is a bearer credential: whoever holds it IS the user.
/// </remarks>
public interface ITokenGenerator
{
    string NewToken();
}

public interface IIdentityRepository
{
    Task<User?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RoleName>> GetRolesAsync(Guid userId, CancellationToken cancellationToken);

    Task<Guid> GetRoleIdAsync(RoleName role, CancellationToken cancellationToken);

    void AddUser(User user);

    void AddUserRole(UserRole userRole);

    void AddLoginEvent(UserLoginEvent loginEvent);

    /// <summary>
    /// Preferences are created WITH the account, in registration's one transaction.
    /// </summary>
    /// <remarks>
    /// It sits on this port rather than on <see cref="IUserPreferencesRepository"/> — which is where it
    /// belongs by name — because the port that owns the transaction must own every insert inside it.
    /// Splitting the write across two repositories and relying on them happening to share one scoped
    /// DbContext is the kind of invisible coupling that works until somebody gives one of them its own
    /// context, and then registration starts producing users with no preferences row.
    ///
    /// And a user with no preferences row is not a small bug: <c>GET /users/me/preferences</c> would
    /// 404 for an account that exists, and `08` forbids a GET from repairing it (a GET must not mutate
    /// server state). The invariant is "every user has exactly one preferences row", and this is where
    /// it is made true.
    /// </remarks>
    void AddPreferences(UserPreferences preferences);

    void AddSession(UserSession session);

    Task<UserSession?> FindSessionByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    /// <summary>
    /// Every session descended from one sign-in. Reuse detection revokes all of them at once — the
    /// thief may already hold a newer token in the same chain, and killing only the replayed one leaves
    /// them signed in while the victim is not.
    /// </summary>
    Task<IReadOnlyCollection<UserSession>> GetFamilyAsync(Guid familyId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UserSession>> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken);

    void AddEmailConfirmationToken(EmailConfirmationToken token);

    Task<EmailConfirmationToken?> FindEmailConfirmationTokenAsync(string tokenHash, CancellationToken cancellationToken);

    void AddPasswordResetToken(PasswordResetToken token);

    Task<PasswordResetToken?> FindPasswordResetTokenAsync(string tokenHash, CancellationToken cancellationToken);

    /// <summary>
    /// Marks every outstanding reset token for this user as used.
    ///
    /// Issuing a second reset link must kill the first. Otherwise "I clicked reset twice because the
    /// mail was slow" leaves two live keys to the account sitting in an inbox, and the older one — the
    /// one more likely to have been forwarded, synced, or read over a shoulder — stays valid.
    /// </summary>
    Task InvalidateOutstandingPasswordResetTokensAsync(Guid userId, CancellationToken cancellationToken);

    Task InvalidateOutstandingEmailConfirmationTokensAsync(Guid userId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IUserPreferencesRepository
{
    Task<UserPreferences?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Saves, but only if nobody else has written this row since the caller read it.
    /// </summary>
    /// <remarks>
    /// Returns <c>false</c> on a conflict rather than throwing, because a conflict is an EXPECTED
    /// outcome — two devices, one account — not an exceptional one, and the caller must decide what to
    /// do about it (see <see cref="Common.Result{T}"/>).
    ///
    /// It takes <paramref name="expectedRowVersion"/> instead of trusting the entity's own RowVersion:
    /// the entity was just loaded from the database, so its RowVersion is by definition CURRENT, and
    /// checking it against itself would always pass. The value that matters is the one the CLIENT last
    /// saw — which is the one it sends back — and comparing against that is the whole mechanism.
    ///
    /// The comparison itself happens in Infrastructure, where EF Core's concurrency token lives. That
    /// is deliberate: <c>DbUpdateConcurrencyException</c> is an EF Core type, and catching it here would
    /// drag the ORM across the boundary CLAUDE.md §3 draws.
    /// </remarks>
    Task<bool> TrySaveAsync(
        UserPreferences preferences,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken);
}

/// <summary>
/// Delivery of transactional mail. Real sending arrives with the confirmation and reset flows; the port
/// exists now so registration cannot be written in a way that assumes it does not.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}

public sealed record EmailMessage(string To, string Subject, string Body);

/// <summary>
/// Builds the links that go in email. The client's base URL is deployment configuration, not something
/// a use case should know — and hardcoding it would mean the reset link in a staging email points at
/// production.
/// </summary>
public interface IAppLinks
{
    string ConfirmEmail(string token);

    string ResetPassword(string token);
}
