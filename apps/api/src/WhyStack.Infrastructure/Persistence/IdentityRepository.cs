using Microsoft.EntityFrameworkCore;
using WhyStack.Application.Abstractions;
using WhyStack.Domain.Identity;
using WhyStack.Domain.Users;

namespace WhyStack.Infrastructure.Persistence;

/// <summary>
/// The EF Core side of <see cref="IIdentityRepository"/>. Every query in the identity domain is here,
/// and nowhere else — which is what lets the Application layer be tested without a database at all.
/// </summary>
public sealed class IdentityRepository(WhyStackDbContext context, IClock clock) : IIdentityRepository
{
    public Task<User?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        context.Users
            .SingleOrDefaultAsync(
                user => user.NormalizedEmail == normalizedEmail && user.DeletedAtUtc == null,
                cancellationToken);

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        context.Users
            .AsNoTracking()
            .AnyAsync(
                user => user.NormalizedEmail == normalizedEmail && user.DeletedAtUtc == null,
                cancellationToken);

    /// <summary>
    /// A join, not two round trips. Loading the UserRole rows and then fetching each Role by id is the
    /// N+1 that everybody writes once: it is invisible with two roles and quietly ruinous with two
    /// hundred thousand logins a day.
    /// </summary>
    public async Task<IReadOnlyCollection<RoleName>> GetRolesAsync(Guid userId, CancellationToken cancellationToken) =>
        await context.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId)
            .Select(userRole => userRole.Role!.Name)
            .ToListAsync(cancellationToken);

    public async Task<Guid> GetRoleIdAsync(RoleName role, CancellationToken cancellationToken)
    {
        var normalized = role.ToString().ToUpperInvariant();

        var id = await context.Roles
            .AsNoTracking()
            .Where(entity => entity.NormalizedName == normalized)
            .Select(entity => entity.Id)
            .SingleOrDefaultAsync(cancellationToken);

        // The roles are seeded by a migration. If one is missing, the database is not the database this
        // code was written against, and continuing would create a user with no role — an account that
        // exists, can sign in, and can do nothing, with no trace of why. Fail loudly (CLAUDE.md 1.6).
        return id == Guid.Empty
            ? throw new InvalidOperationException(
                $"Role '{role}' is not seeded. The database is missing the Schema_Identity migration.")
            : id;
    }

    public Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken) =>
        context.Users.SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public void AddUser(User user) => context.Users.Add(user);

    public void AddUserRole(UserRole userRole) => context.UserRoles.Add(userRole);

    public void AddLoginEvent(UserLoginEvent loginEvent) => context.UserLoginEvents.Add(loginEvent);

    public void AddPreferences(UserPreferences preferences) => context.UserPreferences.Add(preferences);

    public void AddSession(UserSession session) => context.UserSessions.Add(session);

    /// <summary>
    /// Tracked, not AsNoTracking: the caller mutates it (rotation sets ReplacedBySessionId) and saves.
    ///
    /// It also finds REVOKED and ROTATED sessions on purpose. Filtering to usable ones here would be
    /// the natural-looking optimisation and would silently delete reuse detection: a replayed token
    /// would simply not be found, the handler would report "unknown token", the family would survive,
    /// and the thief would keep refreshing.
    /// </summary>
    public Task<UserSession?> FindSessionByRefreshTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken) =>
        context.UserSessions.SingleOrDefaultAsync(
            session => session.RefreshTokenHash == tokenHash,
            cancellationToken);

    public async Task<IReadOnlyCollection<UserSession>> GetFamilyAsync(
        Guid familyId,
        CancellationToken cancellationToken) =>
        await context.UserSessions
            .Where(session => session.FamilyId == familyId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<UserSession>> GetActiveSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await context.UserSessions
            .Where(session => session.UserId == userId && session.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

    public void AddEmailConfirmationToken(EmailConfirmationToken token) =>
        context.EmailConfirmationTokens.Add(token);

    /// <summary>
    /// Finds spent and expired tokens too. Filtering them out here would make a replayed link look
    /// like a link that never existed — the same answer, but the handler would lose the ability to
    /// tell them apart if it ever needed to, and the audit trail would lose it permanently.
    /// </summary>
    public Task<EmailConfirmationToken?> FindEmailConfirmationTokenAsync(
        string tokenHash,
        CancellationToken cancellationToken) =>
        context.EmailConfirmationTokens.SingleOrDefaultAsync(
            token => token.TokenHash == tokenHash,
            cancellationToken);

    public void AddPasswordResetToken(PasswordResetToken token) =>
        context.PasswordResetTokens.Add(token);

    public Task<PasswordResetToken?> FindPasswordResetTokenAsync(
        string tokenHash,
        CancellationToken cancellationToken) =>
        context.PasswordResetTokens.SingleOrDefaultAsync(
            token => token.TokenHash == tokenHash,
            cancellationToken);

    public async Task InvalidateOutstandingPasswordResetTokensAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var outstanding = await context.PasswordResetTokens
            .Where(token => token.UserId == userId && token.UsedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in outstanding)
        {
            token.UsedAtUtc = clock.UtcNow;
        }
    }

    public async Task InvalidateOutstandingEmailConfirmationTokensAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var outstanding = await context.EmailConfirmationTokens
            .Where(token => token.UserId == userId && token.UsedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in outstanding)
        {
            token.UsedAtUtc = clock.UtcNow;
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
