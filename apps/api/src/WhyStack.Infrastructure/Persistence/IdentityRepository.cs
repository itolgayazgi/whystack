using Microsoft.EntityFrameworkCore;
using WhyStack.Application.Abstractions;
using WhyStack.Domain.Identity;

namespace WhyStack.Infrastructure.Persistence;

/// <summary>
/// The EF Core side of <see cref="IIdentityRepository"/>. Every query in the identity domain is here,
/// and nowhere else — which is what lets the Application layer be tested without a database at all.
/// </summary>
public sealed class IdentityRepository(WhyStackDbContext context) : IIdentityRepository
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

    public void AddUser(User user) => context.Users.Add(user);

    public void AddUserRole(UserRole userRole) => context.UserRoles.Add(userRole);

    public void AddLoginEvent(UserLoginEvent loginEvent) => context.UserLoginEvents.Add(loginEvent);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
