using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using WhyStack.Application.Abstractions;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Api.Tests;

/// <summary>
/// Captures every email the API sends, instead of sending it.
/// </summary>
/// <remarks>
/// This is what lets an endpoint test do what a user does: register, open the mail, take the link out
/// of it, and click. Without it, the confirmation and reset flows could only be tested up to the point
/// where the link is generated — which is precisely the point where they are most likely to be wrong.
///
/// It also keeps SMTP out of the test run. The first version of these tests hit the real
/// SmtpEmailSender, and CI has no mail server: two tests failed after a five-second connect timeout,
/// with an error that said nothing about email.
/// </remarks>
public sealed class RecordingEmailSender : IEmailSender
{
    // A locked List, not a ConcurrentBag.
    //
    // ConcurrentBag does not preserve insertion order — it enumerates in an unspecified, roughly
    // per-thread LIFO order — so LastOrDefault() on one is meaningless. The first version used one, and
    // "the last email sent to this address" sometimes came back as the CONFIRMATION mail rather than
    // the RESET mail. The reset then failed with invalid_reset_token, and the error pointed at the
    // handler rather than at the test double.
    private readonly List<EmailMessage> _sent = [];
    private readonly Lock _gate = new();

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _sent.Add(message);
        }

        return Task.CompletedTask;
    }

    public EmailMessage? LastTo(string address)
    {
        lock (_gate)
        {
            return _sent
                .Where(mail => string.Equals(mail.To, address, StringComparison.OrdinalIgnoreCase))
                .LastOrDefault();
        }
    }

    /// <summary>Pulls the token out of the link — exactly what a person clicking it does.</summary>
    public string TokenFrom(EmailMessage mail) =>
        mail.Body.Split("token=")[1].Split('\n')[0].Trim();
}

/// <summary>
/// The real application, against the real database, with a signing key made up for the test run.
/// </summary>
/// <remarks>
/// The API refuses to start without <c>Jwt:SigningKey</c> — by design. That is why this class exists,
/// and the fact that the tests broke the moment that guard was added is the guard proving itself: an
/// environment with no signing key must fail to start, and "environment" includes this one.
/// </remarks>
public class WhyStackApiFactory : WebApplicationFactory<Program>
{
    public const string TestSigningKey =
        "test-only-signing-key-not-a-secret-0123456789abcdef0123456789abcdef";

    public RecordingEmailSender Emails { get; } = new();

    /// <summary>
    /// Overridden by <see cref="RateLimitedApiFactory"/>. Everywhere else it is effectively off.
    ///
    /// Not a weakening of the test suite — a fix for it. Every request in a test process comes from the
    /// same address, so one shared limiter means the rate-limit test starves every other test of its
    /// budget, and they fail for a reason that has nothing to do with what they assert.
    /// </summary>
    protected virtual int AuthPermitLimit => 10_000;

    /// <summary>
    /// Every topic and user a test seeded, so the run can put the database back the way it found it.
    /// </summary>
    /// <remarks>
    /// These tests run against the REAL development database — that is deliberate; an in-memory provider
    /// would not have caught a single one of the FK and index defects this suite has caught. The price is
    /// that anything a test writes is still there afterwards, and published `apitest-*` topics were showing
    /// up in the reader's topic list on the developer's own machine after every run. A test that leaves
    /// fake content in a real database is a test that lies to the next person who opens the app.
    ///
    /// Tracked ids rather than "delete everything matching apitest.%": each test CLASS gets its own factory
    /// instance, so a blanket sweep on teardown would delete rows another class is still using.
    /// </remarks>
    private readonly ConcurrentBag<Guid> _seededTopics = [];
    private readonly ConcurrentBag<Guid> _seededUsers = [];
    private readonly ConcurrentBag<Guid> _seededDomains = [];

    public void TrackTopic(Guid id) => _seededTopics.Add(id);

    public void TrackUser(Guid id) => _seededUsers.Add(id);

    public void TrackDomain(Guid id) => _seededDomains.Add(id);

    public override async ValueTask DisposeAsync()
    {
        await PurgeAsync();
        await base.DisposeAsync();
    }

    private async Task PurgeAsync()
    {
        var topics = _seededTopics.ToArray();
        var users = _seededUsers.ToArray();
        var domains = _seededDomains.ToArray();

        if (topics.Length == 0 && users.Length == 0 && domains.Length == 0) return;

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WhyStackDbContext>();

        // Progress first, and not because of tidiness: UserTopicProgress -> Topic is Restrict on purpose
        // (ADR-0025), so the delete would fail rather than quietly take a reader's history with it. The
        // teardown has to unwind in the same order the constraint demands.
        await context.UserTopicProgress
            .Where(progress => topics.Contains(progress.TopicId) || users.Contains(progress.UserId))
            .ExecuteDeleteAsync();

        await context.UserStreaks.Where(streak => users.Contains(streak.UserId)).ExecuteDeleteAsync();
        await context.Topics.Where(topic => topics.Contains(topic.Id)).ExecuteDeleteAsync();
        await context.Users.Where(user => users.Contains(user.Id)).ExecuteDeleteAsync();

        // After the topics that sit in them, for the same reason.
        await context.KnowledgeDomains.Where(domain => domains.Contains(domain.Id)).ExecuteDeleteAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = TestSigningKey,
                ["Jwt:Issuer"] = "https://api.whystack.test",
                ["Jwt:Audience"] = "https://whystack.test",
                ["App:ClientBaseUrl"] = "https://whystack.test",
                ["RateLimiting:Auth:PermitLimit"] = AuthPermitLimit.ToString(),

                // The prune job is tested where it is the subject (WhyStack.Infrastructure.Tests),
                // against the same database. Leaving it running here would have two test assemblies
                // contending for one sp_getapplock, and the loser would fail for a reason that has
                // nothing to do with what it asserts.
                ["SessionMaintenance:Enabled"] = "false",

            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);
        });
    }

}

/// <summary>The same application, with the rate limit turned down so it can actually be tripped.</summary>
public sealed class RateLimitedApiFactory : WhyStackApiFactory
{
    public const int PermitLimit = 3;

    protected override int AuthPermitLimit => PermitLimit;
}
