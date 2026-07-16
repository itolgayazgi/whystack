using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WhyStack.Application.Abstractions;
using WhyStack.Domain.Content;
using WhyStack.Domain.Identity;
using WhyStack.Domain.Users;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Api.Tests;

/// <summary>
/// A reader's percentage must never fall because WE published something.
/// </summary>
/// <remarks>
/// From the owner's content-loop design: <i>"dün %100'düm, bugün %91 oldum" hissi cezalandırma gibi
/// çalışır. Yeni içerik ödül kapısıdır, borç değil.</i>
///
/// The basamak's denominator was the corpus as it stands right now, so publishing one Junior topic pushed
/// every Junior reader further from the top than they were yesterday — the platform's own productivity,
/// charged to the people who read the most, and charged hardest to whoever had finished everything.
///
/// It is silent by construction: no error, no log, nothing to notice unless you are the reader watching
/// your own number go down. That is why it is tested rather than eyeballed.
/// </remarks>
public class LevelBaselineTests(WhyStackApiFactory factory) : IClassFixture<WhyStackApiFactory>
{
    [Fact]
    public async Task Publishing_a_new_stop_does_not_take_a_finished_reader_backwards()
    {
        var (client, _) = await ReaderAsync();
        var line = await LineAsync();

        var first = await SeedAsync(line, publishedDaysAgo: 10);

        // Arriving stamps the baseline: this is the corpus the reader's percentage is measured against.
        await client.PostAsJsonAsync("/api/v1/progress", new { slug = first, lastBlockOrder = 1 });
        await client.PostAsJsonAsync("/api/v1/progress", new { slug = first, lastBlockOrder = 3, completed = true });

        var before = Rung(await HomeAsync(client), "Senior");

        var total = before.GetProperty("total").GetInt32();
        var fresh = before.GetProperty("fresh").GetInt32();

        Assert.Equal(1, before.GetProperty("completed").GetInt32());

        // We publish. The reader did nothing.
        await SeedAsync(line, publishedDaysAgo: 0);

        var after = Rung(await HomeAsync(client), "Senior");

        // THE ASSERTION THIS FILE EXISTS FOR.
        //
        // Asserted as the INVARIANT rather than as "1/1", and not to dodge a hard number: the basamak is
        // deliberately domain-agnostic — it is the reader's whole ladder — so every sibling test's Junior
        // topic lands in this denominator too. A literal 1 here would be a test about who else ran, and it
        // would go red for a reason that has nothing to do with the rule.
        //
        // The rule is: once a reader has arrived, their denominator is FROZEN. That is exactly this line,
        // and it holds no matter what anybody else publishes — which is the whole promise.
        Assert.Equal(total, after.GetProperty("total").GetInt32());
        Assert.Equal(1, after.GetProperty("completed").GetInt32());

        // And the new stop is not hidden: it is announced as a reward. The design's "10/11 · 1 yeni".
        Assert.Equal(fresh + 1, after.GetProperty("fresh").GetInt32());
    }

    [Fact]
    public async Task A_reader_who_has_never_been_to_a_level_sees_all_of_it()
    {
        var (client, _) = await ReaderAsync();
        var line = await LineAsync();

        await SeedAsync(line, publishedDaysAgo: 5);
        await SeedAsync(line, publishedDaysAgo: 1);

        var rung = Rung(await HomeAsync(client), "Senior");

        // No baseline means no history to protect. The honest denominator is everything published — that is
        // what they will find when they get there. Freezing it at zero would show them an empty ladder.
        //
        // `fresh` is what makes this an assertion rather than a shrug: with no baseline, NOTHING is new,
        // because there is no "since" to be new after.
        Assert.True(rung.GetProperty("total").GetInt32() >= 2);
        Assert.Equal(0, rung.GetProperty("fresh").GetInt32());
    }

    [Fact]
    public async Task The_baseline_is_stamped_once_and_never_moves()
    {
        var (client, userId) = await ReaderAsync();
        var line = await LineAsync();

        var first = await SeedAsync(line, publishedDaysAgo: 10);
        await client.PostAsJsonAsync("/api/v1/progress", new { slug = first, lastBlockOrder = 1 });

        var stamped = await BaselineAsync(userId);

        await SeedAsync(line, publishedDaysAgo: 0);

        // Reading again, AFTER we published. If the baseline moved to now, the new stop would fall inside it
        // and the protection would evaporate on the reader's very next visit — which is every reader, always.
        await client.PostAsJsonAsync("/api/v1/progress", new { slug = first, lastBlockOrder = 2 });

        Assert.Equal(stamped, await BaselineAsync(userId));
    }

    // ── helpers ────────────────────────────────────────────────────────────────────────────────────

    private static async Task<JsonElement> HomeAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/v1/home?language=en");
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
    }

    private static JsonElement Rung(JsonElement home, string level) =>
        Assert.Single(home.GetProperty("levels").EnumerateArray(), rung => rung.GetProperty("level").GetString() == level);

    private async Task<DateTime> BaselineAsync(Guid userId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WhyStackDbContext>();

        return (await context.UserLevelBaselines.SingleAsync(b => b.UserId == userId)).EnteredAtUtc;
    }

        /// <summary>
    /// A line of this test's own, so the map contains this test's stations and nothing else.
    /// </summary>
    /// <remarks>
    /// Not fussiness. "Next" is the first UNSTARTED station on the whole line — a fact about the line, not
    /// about a topic — so a sibling test seeding one more stop on a shared line silently moves it.
    ///
    /// Its AREA is the real seeded `backend`: the isolation this needs is at the line, and inventing an area
    /// too would make the fixture less like the thing it is testing.
    /// </remarks>
    private async Task<string> LineAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WhyStackDbContext>();

        var id = Guid.CreateVersion7();
        var key = $"apitest-{id:N}";

        context.Lines.Add(new Line
        {
            Id = id,
            Key = key,
            Name = "Test Line",
            AreaId = DeterministicId.For("area:backend"),
            Color = "#C9A227",
            SortOrder = 99,
        });

        await context.SaveChangesAsync();

        factory.TrackLine(id);

        return key;
    }

    private async Task<(HttpClient Client, Guid UserId)> ReaderAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WhyStackDbContext>();

        var id = Guid.CreateVersion7();

        var user = new User
        {
            Id = id,
            Email = $"apitest.{id:N}@example.com",
            NormalizedEmail = $"APITEST.{id:N}@EXAMPLE.COM",
            DisplayName = "Reader",
            PasswordHash = "irrelevant",
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        factory.TrackUser(id);

        var token = scope.ServiceProvider
            .GetRequiredService<IAccessTokenIssuer>()
            .Issue(user, [RoleName.RegisteredUser])
            .Token;

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return (client, id);
    }

    /// <summary>
    /// One published SENIOR topic, published at a stated point in the past.
    /// </summary>
    /// <remarks>
    /// Senior because no other test class seeds it (the rest use Junior and Expert), and the basamak chart
    /// is deliberately domain-agnostic — it is the reader's whole ladder, so a per-test domain isolates the
    /// roadmap but not this. `fresh` counts everything outside the baseline, which includes topics a
    /// PARALLEL test class published a millisecond ago. On Junior, the arithmetic below was a coin flip.
    ///
    /// The `total` assertions would survive either way — a frozen denominator is frozen against everybody —
    /// but a test that is only half robust teaches you to distrust it when it goes red.
    /// </remarks>
    private async Task<string> SeedAsync(string line, int publishedDaysAgo)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WhyStackDbContext>();

        var lineId = await context.Lines.Where(candidate => candidate.Key == line).Select(candidate => candidate.Id).SingleAsync();

        var key = $"apitest.{Guid.CreateVersion7():N}";
        var slug = key.Replace('.', '-');

        var topic = new Topic
        {
            Id = Guid.CreateVersion7(),
            StableKey = key,
            Slug = slug,
            LineId = lineId,
            Category = TopicCategory.Concept,
            Archetype = Archetype.Concept,
            DefaultLevel = SkillLevel.Senior,
            DefaultTitle = "A stop",
            CreatedAtUtc = DateTime.UtcNow,
        };

        var version = new TopicVersion
        {
            Id = Guid.CreateVersion7(),
            TopicId = topic.Id,
            VersionNumber = 1,
            Status = ContentStatus.Published,
            CanonicalLanguageCode = "en",
            EstimatedReadingMinutes = 6,
            LastReviewedOn = new DateOnly(2026, 7, 14),
            CreatedAtUtc = DateTime.UtcNow,

            // The whole point of the fix: WHEN a stop opened decides whether it is inside a reader's
            // baseline. A seed that left this null would prove nothing.
            PublishedAtUtc = DateTime.UtcNow.AddDays(-publishedDaysAgo),
        };

        version.Translations.Add(new TopicTranslation
        {
            Id = Guid.CreateVersion7(),
            TopicVersionId = version.Id,
            LanguageCode = "en",
            Title = "A stop",
            Status = TranslationStatus.HumanDraft,
            CreatedAtUtc = DateTime.UtcNow,
        });

        for (var order = 1; order <= 3; order++)
        {
            version.Blocks.Add(new TopicBlock
            {
                Id = Guid.CreateVersion7(),
                TopicVersionId = version.Id,
                Order = order,
                Type = BlockType.Concept,
                LanguageCode = "en",
                DataJson = """{"title":"A block","body":"Words."}""",
                CreatedAtUtc = DateTime.UtcNow,
            });
        }

        topic.Versions.Add(version);
        context.Topics.Add(topic);
        await context.SaveChangesAsync();

        factory.TrackTopic(topic.Id);

        return slug;
    }
}
