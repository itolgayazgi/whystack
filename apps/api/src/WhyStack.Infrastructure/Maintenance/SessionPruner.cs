using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Infrastructure.Maintenance;

/// <summary>
/// Deletes expired rows from <c>UserSessions</c>, in batches.
/// </summary>
/// <remarks>
/// <para><b>Why this exists.</b> Every refresh ROTATES the token, and rotation writes a new row. An
/// active device refreshes whenever its fifteen-minute access token expires — roughly 32 times a day —
/// so one device produces about 12,000 rows a year. Ten thousand users on two devices produce about
/// 234 million. Nothing deleted them. That is the class of bug nobody notices for two years and then
/// cannot fix quickly.</para>
///
/// <para><b>Why deleting is safe.</b> Only rows past <c>ExpiresAtUtc</c> are removed, and an expired
/// refresh token is already rejected on expiry alone (<c>UserSession.IsUsable</c>). Removing the row
/// cannot weaken reuse detection: the token it describes was going to be refused either way. What
/// changes is only the REASON — a replay of a pruned token reads as "unknown token" rather than
/// "reuse" — and by then the token is expired, so there is nothing left to detect.</para>
///
/// <para><b>Why the full rotation chain is kept until then.</b> The alternative design — one row per
/// session, updated in place — would hold ~20,000 rows instead of ~19 million, but it can only
/// recognise a replay of the IMMEDIATELY PRECEDING token. Keeping the chain for the refresh token's
/// whole lifetime means <b>every</b> replay inside that window is caught, the family is revoked, and
/// an alarm is raised. That is the stronger security property, and 19 million rows is a table SQL
/// Server finds unremarkable.</para>
///
/// <para><b>Why raw SQL</b> (`07` Raw SQL Rules — permitted for bulk updates, with a performance
/// rationale): loading 640,000 entities into the change tracker to delete them would be absurd. The
/// statement is parameterised, and the batching is the point of it — see below.</para>
/// </remarks>
public sealed partial class SessionPruner(WhyStackDbContext context, ILogger<SessionPruner> logger)
{
    /// <summary>
    /// One statement per batch, and a small pause between them.
    ///
    /// <b>A single <c>DELETE</c> of 640,000 rows is how you take the site down.</b> SQL Server escalates
    /// row locks to a TABLE lock past roughly 5,000 of them — at which point nobody can sign in, refresh,
    /// or log out until it finishes, and the transaction log grows to hold the whole thing. Batching
    /// keeps every transaction small enough to stay below escalation, and short enough that a lock is
    /// never held long.
    /// </summary>
    private const string DeleteBatch =
        """
        DELETE TOP (@batchSize) FROM [UserSessions]
        WHERE [ExpiresAtUtc] < @cutoffUtc;
        """;

    /// <summary>
    /// Claims the right to prune, so that N API instances do not all do the same work at once.
    ///
    /// The same primitive EF Core uses for migrations. Session-scoped, so it releases itself if the
    /// process dies holding it — a maintenance job that can deadlock the next deploy is worse than one
    /// that occasionally runs twice.
    /// </summary>
    private const string TryAcquireLock =
        """
        DECLARE @result int;
        EXEC @result = sp_getapplock
            @Resource = 'WhyStack_SessionPrune',
            @LockMode = 'Exclusive',
            @LockOwner = 'Session',
            @LockTimeout = 0;
        SELECT @result;
        """;

    private const string ReleaseLock =
        """
        EXEC sp_releaseapplock @Resource = 'WhyStack_SessionPrune', @LockOwner = 'Session';
        """;

    public async Task<int> PruneAsync(
        DateTime cutoffUtc,
        int batchSize,
        TimeSpan pauseBetweenBatches,
        CancellationToken cancellationToken)
    {
        // A single connection for the whole run: sp_getapplock with @LockOwner = 'Session' is scoped to
        // the connection, so releasing it on a different one from the pool would release nothing.
        await context.Database.OpenConnectionAsync(cancellationToken);

        // Track whether we actually got it. Releasing a lock you never held is not a no-op — SQL Server
        // raises "Cannot release the application lock ... because it is not currently held", and the
        // BackgroundService would log a prune FAILURE every hour on every instance that correctly
        // skipped its turn. The job would look broken while doing exactly the right thing.
        var holdsLock = false;

        try
        {
            holdsLock = await TryAcquireLockAsync(cancellationToken);

            if (!holdsLock)
            {
                AnotherInstanceIsPruning(logger);
                return 0;
            }

            var deleted = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                var batch = await context.Database.ExecuteSqlRawAsync(
                    DeleteBatch,
                    [
                        new Microsoft.Data.SqlClient.SqlParameter("@batchSize", batchSize),
                        new Microsoft.Data.SqlClient.SqlParameter("@cutoffUtc", cutoffUtc),
                    ],
                    cancellationToken);

                deleted += batch;

                if (batch < batchSize)
                {
                    break;
                }

                // Room for everyone else. A prune job that starves the requests it exists to protect is
                // a prune job that gets switched off.
                await Task.Delay(pauseBetweenBatches, cancellationToken);
            }

            if (deleted > 0)
            {
                Pruned(logger, deleted, cutoffUtc);
            }

            return deleted;
        }
        finally
        {
            try
            {
                if (holdsLock)
                {
                    // CancellationToken.None: the lock must be released even when we are shutting down.
                    // Passing the stopping token here would abandon it on every deploy, and the next
                    // instance would find the resource held by a connection that no longer exists.
                    await context.Database.ExecuteSqlRawAsync(ReleaseLock, CancellationToken.None);
                }
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }
    }

    private async Task<bool> TryAcquireLockAsync(CancellationToken cancellationToken)
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = TryAcquireLock;

        var result = await command.ExecuteScalarAsync(cancellationToken);

        // sp_getapplock: 0 = granted, 1 = granted after waiting. Anything negative means somebody else
        // holds it, which is not an error — it means there is nothing for us to do.
        return result is int code && code >= 0;
    }

    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Information,
        Message = "Pruned {Deleted} expired session row(s) older than {CutoffUtc:u}.")]
    private static partial void Pruned(ILogger logger, int deleted, DateTime cutoffUtc);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Debug,
        Message = "Another instance holds the session-prune lock. Skipping this run.")]
    private static partial void AnotherInstanceIsPruning(ILogger logger);
}
