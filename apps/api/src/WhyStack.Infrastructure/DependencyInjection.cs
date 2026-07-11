using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Infrastructure;

/// <summary>
/// Registers everything Infrastructure owns. The API calls this once and never learns what is inside:
/// no EF Core type, no connection string, no provider name reaches <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>The health-check tag that means "this dependency must be up before we accept traffic".</summary>
    public const string ReadinessTag = "ready";

    public const string ConnectionStringName = "WhyStackDatabase";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        // Starting with no database configured and discovering it on the first request is worse than
        // not starting at all: the app looks healthy, and fails only when a user touches it.
        // Fail here, loudly, at boot (CLAUDE.md 1.6).
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured. "
                    + "Set ConnectionStrings__WhyStackDatabase, or add it to user secrets. "
                    + "See apps/api/README.md.");
        }

        services.AddDbContext<WhyStackDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServer =>
                {
                    // SQL Server drops connections for reasons that are nobody's fault — a failover, a
                    // pool reset, a container still waking up. Without this, those surface as 500s.
                    // It retries only errors SQL Server itself marks as transient, so a genuine bug is
                    // still a bug and still fails immediately.
                    sqlServer.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);

                    sqlServer.MigrationsAssembly(typeof(WhyStackDbContext).Assembly.FullName);
                }));

        // Tagged as readiness, not liveness. A database outage means "do not send me traffic", not
        // "kill and restart me" — restarting the API does not fix SQL Server, it just adds an outage.
        services
            .AddHealthChecks()
            .AddDbContextCheck<WhyStackDbContext>(name: "sql-server", tags: [ReadinessTag]);

        return services;
    }
}
