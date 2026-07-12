using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WhyStack.Domain.Identity;
using WhyStack.Infrastructure.Maintenance;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Infrastructure.Tests;

/// <summary>
/// A prune job that deletes the wrong row silently signs people out. These run against a real SQL
/// Server for exactly that reason.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class SessionPrunerTests(DatabaseFixture database) : IAsyncLifetime
{
    private static readonly DateTime Now = new(2026, 7, 12, 12, 0, 0, DateTimeKind.Utc);

    private readonly List<Guid> _userIds = [];

    /// <summary>The whole run shares one database. Every test cleans up after itself, by user.</summary>
    public async Task DisposeAsync()
    {
        await using var context = database.NewContext();

        foreach (var userId in _userIds)
        {
            await context.Users.Where(user => user.Id == userId).ExecuteDeleteAsync();
        }
    }

    public Task InitializeAsync() => Task.CompletedTask;

    private async Task<Guid> GivenAUserAsync(WhyStackDbContext context)
    {
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = $"prune-{Guid.CreateVersion7():N}@example.com",
            NormalizedEmail = $"PRUNE-{Guid.CreateVersion7():N}@EXAMPLE.COM",
            PasswordHash = "hash",
            CreatedAtUtc = Now,
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        _userIds.Add(user.Id);
        return user.Id;
    }

    private static UserSession Session(Guid userId, DateTime expiresAtUtc, DateTime? revokedAtUtc = null) => new()
    {
        Id = Guid.CreateVersion7(),
        UserId = userId,
        FamilyId = Guid.CreateVersion7(),
        RefreshTokenHash = Guid.CreateVersion7().ToString("N") + Guid.CreateVersion7().ToString("N"),
        CreatedAtUtc = Now.AddDays(-40),
        ExpiresAtUtc = expiresAtUtc,
        RevokedAtUtc = revokedAtUtc,
    };

    private static SessionPruner PrunerFor(WhyStackDbContext context) =>
        new(context, NullLogger<SessionPruner>.Instance);

    private static Task<int> PruneAsync(WhyStackDbContext context, DateTime cutoffUtc, int batchSize = 2_000) =>
        PrunerFor(context).PruneAsync(cutoffUtc, batchSize, TimeSpan.Zero, CancellationToken.None);

    [Fact]
    public async Task Deletes_sessions_whose_token_has_expired_past_the_cutoff()
    {
        await using var context = database.NewContext();
        var userId = await GivenAUserAsync(context);

        var expired = Session(userId, expiresAtUtc: Now.AddDays(-10));
        context.UserSessions.Add(expired);
        await context.SaveChangesAsync();

        await PruneAsync(context, cutoffUtc: Now.AddDays(-7));

        Assert.Null(await context.UserSessions.AsNoTracking()
            .SingleOrDefaultAsync(session => session.Id == expired.Id));
    }

    /// <summary>
    /// The assertion that matters. A prune job that deletes a LIVE session is a job that silently signs
    /// people out — and they would blame the app, not the cron.
    /// </summary>
    [Fact]
    public async Task Never_touches_a_session_that_is_still_valid()
    {
        await using var context = database.NewContext();
        var userId = await GivenAUserAsync(context);

        var live = Session(userId, expiresAtUtc: Now.AddDays(20));
        context.UserSessions.Add(live);
        await context.SaveChangesAsync();

        await PruneAsync(context, cutoffUtc: Now.AddDays(-7));

        Assert.NotNull(await context.UserSessions.AsNoTracking()
            .SingleOrDefaultAsync(session => session.Id == live.Id));
    }

    /// <summary>
    /// The retention window: expired, but not expired LONG ago. Kept so an operator investigating last
    /// week still has the rows in front of them.
    /// </summary>
    [Fact]
    public async Task Keeps_a_recently_expired_session_inside_the_retention_window()
    {
        await using var context = database.NewContext();
        var userId = await GivenAUserAsync(context);

        // Expired two days ago; the cutoff is seven days ago. It stays.
        var recentlyExpired = Session(userId, expiresAtUtc: Now.AddDays(-2));
        context.UserSessions.Add(recentlyExpired);
        await context.SaveChangesAsync();

        await PruneAsync(context, cutoffUtc: Now.AddDays(-7));

        Assert.NotNull(await context.UserSessions.AsNoTracking()
            .SingleOrDefaultAsync(session => session.Id == recentlyExpired.Id));
    }

    /// <summary>
    /// A revoked session that has NOT expired is kept. Revocation and expiry are different things, and
    /// the cutoff is about expiry: deleting a revoked-but-unexpired row would remove a rotated ancestor
    /// that reuse detection can still catch a replay of.
    /// </summary>
    [Fact]
    public async Task Keeps_a_revoked_session_whose_token_has_not_expired_yet()
    {
        await using var context = database.NewContext();
        var userId = await GivenAUserAsync(context);

        var revokedButLive = Session(userId, expiresAtUtc: Now.AddDays(20), revokedAtUtc: Now.AddDays(-1));
        context.UserSessions.Add(revokedButLive);
        await context.SaveChangesAsync();

        await PruneAsync(context, cutoffUtc: Now.AddDays(-7));

        Assert.NotNull(await context.UserSessions.AsNoTracking()
            .SingleOrDefaultAsync(session => session.Id == revokedButLive.Id));
    }

    /// <summary>
    /// More rows than one batch holds. This is the loop: without it, only the first batch would ever be
    /// deleted and the table would grow anyway — slower, and with a job that reports success.
    /// </summary>
    [Fact]
    public async Task Deletes_everything_even_when_it_takes_several_batches()
    {
        await using var context = database.NewContext();
        var userId = await GivenAUserAsync(context);

        const int total = 25;
        const int batchSize = 10;

        for (var i = 0; i < total; i++)
        {
            context.UserSessions.Add(Session(userId, expiresAtUtc: Now.AddDays(-30)));
        }

        await context.SaveChangesAsync();

        var deleted = await PruneAsync(context, cutoffUtc: Now.AddDays(-7), batchSize: batchSize);

        Assert.True(deleted >= total, $"Expected at least {total} deleted, got {deleted}.");

        Assert.Empty(await context.UserSessions.AsNoTracking()
            .Where(session => session.UserId == userId)
            .ToListAsync());
    }

    /// <summary>
    /// Nothing to do is not a failure. A job that throws on an empty table wakes somebody up at 3am for
    /// the healthiest possible state.
    /// </summary>
    [Fact]
    public async Task Deleting_nothing_is_fine()
    {
        await using var context = database.NewContext();
        await GivenAUserAsync(context);

        var deleted = await PruneAsync(context, cutoffUtc: Now.AddYears(-50));

        Assert.Equal(0, deleted);
    }

    /// <summary>
    /// Two instances, one lock. The second finds it taken and does nothing — which is correct, not an
    /// error: the first one is already deleting the same rows.
    /// </summary>
    [Fact]
    public async Task Two_instances_do_not_prune_at_the_same_time()
    {
        await using var first = database.NewContext();
        await using var second = database.NewContext();

        var userId = await GivenAUserAsync(first);

        for (var i = 0; i < 5; i++)
        {
            first.UserSessions.Add(Session(userId, expiresAtUtc: Now.AddDays(-30)));
        }

        await first.SaveChangesAsync();

        // Hold the lock on `first`'s connection, then try to prune on `second`'s.
        await first.Database.OpenConnectionAsync();

        try
        {
            await first.Database.ExecuteSqlRawAsync(
                """
                DECLARE @r int;
                EXEC @r = sp_getapplock @Resource = 'WhyStack_SessionPrune',
                    @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 0;
                """);

            var deleted = await PruneAsync(second, cutoffUtc: Now.AddDays(-7));

            Assert.Equal(0, deleted);

            // And the rows are untouched — the second instance really did nothing, rather than deleting
            // half of them and reporting zero.
            Assert.Equal(5, await second.UserSessions.AsNoTracking()
                .CountAsync(session => session.UserId == userId));
        }
        finally
        {
            await first.Database.ExecuteSqlRawAsync(
                "EXEC sp_releaseapplock @Resource = 'WhyStack_SessionPrune', @LockOwner = 'Session';");
            await first.Database.CloseConnectionAsync();
        }
    }
}
