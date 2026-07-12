using Microsoft.EntityFrameworkCore;
using WhyStack.Application.Abstractions;
using WhyStack.Domain.Users;

namespace WhyStack.Infrastructure.Persistence;

public sealed class UserPreferencesRepository(WhyStackDbContext context) : IUserPreferencesRepository
{
    public Task<UserPreferences?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        context.UserPreferences
            .FirstOrDefaultAsync(preferences => preferences.UserId == userId, cancellationToken);

    public async Task<bool> TrySaveAsync(
        UserPreferences preferences,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken)
    {
        // The whole mechanism, in one line.
        //
        // EF Core builds the UPDATE's WHERE clause from the ORIGINAL values of the concurrency token —
        // not from the current ones. The entity was loaded from the database a moment ago, so its
        // original RowVersion is by definition the LATEST one, and comparing it against itself would
        // always match. Overwriting the original with what the CLIENT last saw is what turns the UPDATE
        // into "...WHERE RowVersion = <the version you were looking at>", which is the only version that
        // can honestly answer "has anyone changed this since?".
        //
        // Get this wrong and everything still compiles, every test that only checks the happy path still
        // passes, and optimistic concurrency silently does nothing at all. That is why there is a test
        // that actually races two writers.
        context.Entry(preferences).Property(entity => entity.RowVersion).OriginalValue = expectedRowVersion;

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Zero rows matched: somebody wrote this row after the caller read it.
            //
            // Caught HERE, and converted to a plain bool, rather than allowed to escape into the
            // Application layer — DbUpdateConcurrencyException is an EF Core type, and letting a use case
            // catch it would drag the ORM across the boundary CLAUDE.md §3 draws. The Application layer
            // is told "no", not "no, because of Entity Framework".
            //
            // The DbContext is left holding a failed change, so it must not be reused for another write
            // in this request. It is scoped to the request and this is the last thing the request does
            // with it, so that is true by construction — the endpoint maps the false to a 409 and stops.
            return false;
        }
    }
}
