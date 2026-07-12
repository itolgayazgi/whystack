using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WhyStack.Infrastructure.Maintenance;

public sealed class SessionMaintenanceOptions
{
    public const string SectionName = "SessionMaintenance";

    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Hourly. Not because the rows are urgent — they are not — but because a job that runs once a day
    /// deletes a day's worth in one go, and a big delete is exactly what the batching exists to avoid.
    /// Little and often keeps every run boring.
    /// </summary>
    public TimeSpan Interval { get; init; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Below SQL Server's lock-escalation threshold (~5,000 row locks), where it gives up on locking
    /// rows and takes the whole table. Cross that line and nobody can sign in until the delete finishes.
    /// </summary>
    public int BatchSize { get; init; } = 2_000;

    public TimeSpan PauseBetweenBatches { get; init; } = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// Kept for a while AFTER the token expires. It buys nothing security-wise — an expired token is
    /// refused regardless — but it means an operator investigating "what happened to this account last
    /// Tuesday" still has the session rows in front of them. A week is cheap.
    /// </summary>
    public TimeSpan RetentionAfterExpiry { get; init; } = TimeSpan.FromDays(7);
}

/// <summary>
/// Runs <see cref="SessionPruner"/> on a timer, forever.
/// </summary>
/// <remarks>
/// It never throws. A maintenance job that takes the API down with it is worse than one that skips a
/// run — the rows can wait an hour, the users cannot. Failures are logged at Error, loudly, so a run
/// that has been failing for a week is visible rather than silent (CLAUDE.md 1.6).
/// </remarks>
public sealed partial class SessionPruneService(
    IServiceScopeFactory scopeFactory,
    IOptions<SessionMaintenanceOptions> options,
    TimeProvider timeProvider,
    ILogger<SessionPruneService> logger) : BackgroundService
{
    private readonly SessionMaintenanceOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            Disabled(logger);
            return;
        }

        using var timer = new PeriodicTimer(_options.Interval, timeProvider);

        // Once at startup, then on the timer. A process that has been down for a day should not wait
        // another hour before catching up.
        await PruneOnceAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PruneOnceAsync(stoppingToken);
        }
    }

    private async Task PruneOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            // A scope per run: the DbContext is scoped, and a BackgroundService is a singleton. Holding
            // one DbContext for the process lifetime is the classic way to leak an ever-growing change
            // tracker and a connection that is never returned to the pool.
            await using var scope = scopeFactory.CreateAsyncScope();

            var pruner = scope.ServiceProvider.GetRequiredService<SessionPruner>();
            var cutoff = timeProvider.GetUtcNow().UtcDateTime - _options.RetentionAfterExpiry;

            await pruner.PruneAsync(
                cutoff,
                _options.BatchSize,
                _options.PauseBetweenBatches,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Shutdown. Not a failure.
        }
        catch (Exception exception)
        {
            // Swallowed on purpose, and LOUDLY. An unhandled exception from a BackgroundService takes
            // the whole host down by default — so a transient SQL timeout at 3am would kill the API.
            // The rows can wait an hour. The users cannot.
            PruneFailed(logger, exception);
        }
    }

    [LoggerMessage(EventId = 2002, Level = LogLevel.Error, Message = "Session prune failed. It will run again on the next tick.")]
    private static partial void PruneFailed(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Warning, Message = "Session pruning is DISABLED. UserSessions will grow without bound.")]
    private static partial void Disabled(ILogger logger);
}
