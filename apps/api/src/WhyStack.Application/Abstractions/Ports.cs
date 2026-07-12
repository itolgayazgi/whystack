using WhyStack.Domain.Identity;

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

public interface IIdentityRepository
{
    Task<User?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RoleName>> GetRolesAsync(Guid userId, CancellationToken cancellationToken);

    Task<Guid> GetRoleIdAsync(RoleName role, CancellationToken cancellationToken);

    void AddUser(User user);

    void AddUserRole(UserRole userRole);

    void AddLoginEvent(UserLoginEvent loginEvent);

    Task SaveChangesAsync(CancellationToken cancellationToken);
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
