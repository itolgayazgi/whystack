using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using WhyStack.Application.Abstractions;
using WhyStack.Application.Content;
using WhyStack.Application.Progress;
using WhyStack.Application.Roadmap;
using WhyStack.Application.Content.Authoring;
using WhyStack.Application.Identity.Confirmation;
using WhyStack.Application.Identity.Login;
using WhyStack.Application.Identity.Logout;
using WhyStack.Application.Identity.Passwords;
using WhyStack.Application.Identity.Refresh;
using WhyStack.Application.Identity.Register;
using WhyStack.Application.Users.Preferences;
using WhyStack.Application.Users.Profile;
using WhyStack.Application.Identity.Sessions;
using WhyStack.Application.Identity.Tokens;
using WhyStack.Infrastructure.Identity;
using WhyStack.Infrastructure.Maintenance;
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
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        AddPersistence(services, configuration);
        AddIdentity(services, configuration, environment);
        AddUseCases(services);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
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

        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();

        AddContent(services);
        AddMaintenance(services, configuration);
    }

    private static void AddContent(IServiceCollection services)
    {
        // The Markdown is in the database now (ADR-0020). There is no file to read, no path to configure and
        // no cache keyed by a content hash — the row IS the content.
        services.AddScoped<ITopicRepository, TopicRepository>();
        services.AddScoped<IContentAuthoringRepository, ContentAuthoringRepository>();
        services.AddScoped<IProgressRepository, ProgressRepository>();
        services.AddScoped<IRoadmapRepository, RoadmapRepository>();
    }

    private static void AddMaintenance(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<SessionMaintenanceOptions>()
            .Bind(configuration.GetSection(SessionMaintenanceOptions.SectionName))
            .Validate(options => options.BatchSize is > 0 and <= 5_000,
                "SessionMaintenance:BatchSize must be between 1 and 5000. Past roughly 5000 row locks, "
                    + "SQL Server escalates to a TABLE lock — and nobody can sign in until the delete finishes.")
            .Validate(options => options.Interval > TimeSpan.Zero, "SessionMaintenance:Interval must be positive.")
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<SessionPruner>();
        services.AddHostedService<SessionPruneService>();
    }

    private static void AddIdentity(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.SigningKey) && options.SigningKey.Length >= 32,
                "Jwt:SigningKey is missing or shorter than 32 characters. HMAC-SHA256's security is bounded "
                    + "by the key, and a short key is a forgeable token for any user, including an Administrator. "
                    + "Set it in user secrets — never in appsettings.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Issuer) && !string.IsNullOrWhiteSpace(options.Audience),
                "Jwt:Issuer and Jwt:Audience are required.")
            // Validate at startup, not on the first login. A misconfigured signing key must stop the
            // process, not surface as a 500 to whoever happened to try signing in first.
            .ValidateOnStart();

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
        services.AddSingleton<ITokenHasher, Sha256TokenHasher>();
        services.AddSingleton<ITokenGenerator, CryptoRandomTokenGenerator>();
        services.AddScoped<IAccessTokenIssuer, JwtAccessTokenIssuer>();

        services
            .AddOptions<AppOptions>()
            .Bind(configuration.GetSection(AppOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(options.ClientBaseUrl, UriKind.Absolute, out _),
                "App:ClientBaseUrl must be an absolute URL. It is where the confirmation and reset links "
                    + "point, and a wrong one sends every user in this environment to a different one.")
            .ValidateOnStart();

        services.AddSingleton<IAppLinks, AppLinks>();

        AddEmail(services, configuration, environment);
    }

    private static void AddEmail(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var smtp = configuration.GetSection(SmtpOptions.SectionName);

        // An environment with no mail server configured must FAIL, not fall back to a logger. A silent
        // no-op sender means registration looks healthy while nobody ever receives a confirmation, and
        // the first person to notice is a user who cannot sign in and has no idea why (CLAUDE.md 1.6).
        //
        // The only exception is a developer who has not run the setup script yet: they get the logging
        // sender, and it shouts EMAIL NOT SENT on every call rather than pretending.
        if (!smtp.Exists() || string.IsNullOrWhiteSpace(smtp["Host"]))
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "Smtp:Host is not configured. This environment cannot send confirmation or password "
                        + "reset email, and must not accept registrations. Configure SMTP, or run in Development.");
            }

            services.AddSingleton<IEmailSender, LoggingEmailSender>();
            return;
        }

        services
            .AddOptions<SmtpOptions>()
            .Bind(smtp)
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.FromAddress),
                "Smtp:FromAddress is required.")
            .ValidateOnStart();

        services.AddSingleton<IEmailSender, SmtpEmailSender>();
    }

    private static void AddUseCases(IServiceCollection services)
    {
        services.AddScoped<SessionService>();
        services.AddScoped<SingleUseTokenService>();
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<ConfirmEmailHandler>();
        services.AddScoped<ResendConfirmationHandler>();
        services.AddScoped<ForgotPasswordHandler>();
        services.AddScoped<ResetPasswordHandler>();
        services.AddScoped<GetCurrentUserHandler>();
        services.AddScoped<GetPreferencesHandler>();
        services.AddScoped<UpdatePreferencesHandler>();
        services.AddScoped<ListTopicsHandler>();
        services.AddScoped<GetTopicHandler>();
        services.AddScoped<SaveTopicHandler>();
        services.AddScoped<RecordProgressHandler>();
        services.AddScoped<GetHomeHandler>();
        services.AddScoped<GetRoadmapHandler>();
        services.AddScoped<GetLinesHandler>();
        services.AddScoped<TransitionTopicHandler>();
        services.AddScoped<ValidateTopicHandler>();
    }
}
