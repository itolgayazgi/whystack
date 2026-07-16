using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using WhyStack.Application.Abstractions;
using WhyStack.Domain.Content;
using WhyStack.Domain.Identity;
using WhyStack.Domain.Users;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Api.Tests;

/// <summary>
/// Progress and the streak (ADR-0025), against the real application and the real database.
/// </summary>
/// <remarks>
/// The premise of every assertion here is that <b>the client is not a source of truth.</b> The app says "I
/// am at block 40"; the app is a program anybody can replace with three lines of curl, and the streak is a
/// number readers will care about enough to want to keep. So the position is clamped, the day comes from
/// the server's clock, and completion is only ever what the reader actually claimed.
/// </remarks>
public class ProgressEndpointsTests(WhyStackApiFactory factory) : IClassFixture<WhyStackApiFactory>
{
    [Fact]
    public async Task Progress_belongs_to_a_person_so_it_needs_one()
    {
        var response = await factory.CreateClient()
            .PostAsJsonAsync("/api/v1/progress", new { slug = "anything", lastBlockOrder = 1 });

        // Anonymous progress has nowhere to go. A 401 here is the group's RequireAuthorization proving it
        // covers this route — the failure mode being guarded is a future route added without it.
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await factory.CreateClient().GetAsync("/api/v1/home")).StatusCode);
    }

    [Fact]
    public async Task A_position_past_the_end_of_the_topic_is_clamped_to_what_exists()
    {
        var (client, _) = await ReaderAsync();
        var slug = await SeedAsync(blocks: 4);

        var response = await client.PostAsJsonAsync(
            "/api/v1/progress",
            new { slug, lastBlockOrder = 9_999 });

        response.EnsureSuccessStatusCode();

        var data = await DataOf(response);

        // THE ASSERTION THIS FILE EXISTS FOR.
        //
        // Trusting the number costs nothing to send and gives a progress bar at 250%, a "finished" reader on
        // a topic they never opened, and a basamak chart that is decoration. The server counts the blocks
        // itself; the client's number only ever narrows.
        Assert.Equal(4, data.GetProperty("lastBlockOrder").GetInt32());
        Assert.Equal(4, data.GetProperty("totalBlocks").GetInt32());
    }

    [Fact]
    public async Task Going_back_to_re_read_does_not_lose_your_place()
    {
        var (client, _) = await ReaderAsync();
        var slug = await SeedAsync(blocks: 6);

        await client.PostAsJsonAsync("/api/v1/progress", new { slug, lastBlockOrder = 5 });

        var response = await client.PostAsJsonAsync("/api/v1/progress", new { slug, lastBlockOrder = 1 });

        // "Kaldığın yer" is the FURTHEST point, not the latest. A reader who scrolls up to check the hook
        // again has not un-read the topic — and the home screen offering to restart them at block 1 would
        // be the product punishing them for re-reading, which is the entire behaviour we want.
        Assert.Equal(5, (await DataOf(response)).GetProperty("lastBlockOrder").GetInt32());
    }

    [Fact]
    public async Task Reaching_the_last_block_is_not_a_claim_to_have_understood_it()
    {
        var (client, _) = await ReaderAsync();
        var slug = await SeedAsync(blocks: 3);

        var response = await client.PostAsJsonAsync("/api/v1/progress", new { slug, lastBlockOrder = 3 });

        // ADR-0025: completion is the READER'S claim, never inferred. Scrolling to the bottom is evidence of
        // scrolling. The promise is "neden", and a platform that marks you as understanding something because
        // your thumb reached the end is measuring the thumb.
        Assert.False((await DataOf(response)).GetProperty("isCompleted").GetBoolean());

        var claimed = await client.PostAsJsonAsync(
            "/api/v1/progress",
            new { slug, lastBlockOrder = 3, completed = true });

        Assert.True((await DataOf(claimed)).GetProperty("isCompleted").GetBoolean());
    }

    [Fact]
    public async Task An_ecosystem_that_does_not_exist_is_refused_rather_than_stored()
    {
        var (client, _) = await ReaderAsync();
        var slug = await SeedAsync(blocks: 3);

        var response = await client.PostAsJsonAsync(
            "/api/v1/progress",
            new { slug, ecosystemKey = "dotnett", lastBlockOrder = 1 });

        // Storing it would be worse than failing: the row is written, the reader is told it worked, and it
        // matches nothing ever again. Invisible, and permanent. The typo has to fail loudly, here.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("resource_not_found", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Progress_against_a_draft_is_progress_against_something_nobody_can_read()
    {
        var (client, _) = await ReaderAsync();
        var slug = await SeedAsync(blocks: 3, status: ContentStatus.AiDraft);

        var response = await client.PostAsJsonAsync("/api/v1/progress", new { slug, lastBlockOrder = 1 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task A_negative_position_is_a_validation_failure_not_a_clamp()
    {
        var (client, _) = await ReaderAsync();
        var slug = await SeedAsync(blocks: 3);

        var response = await client.PostAsJsonAsync("/api/v1/progress", new { slug, lastBlockOrder = -1 });

        // Unlike an inflated position — which is an off-by-one against a freshly edited topic often enough to
        // forgive — there is no reading of "block -1" that is an honest mistake. `08`: 422, Problem Details.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Reading_starts_a_streak_and_reading_again_the_same_day_does_not_inflate_it()
    {
        var (client, _) = await ReaderAsync();
        var slug = await SeedAsync(blocks: 5);

        await client.PostAsJsonAsync("/api/v1/progress", new { slug, lastBlockOrder = 1 });
        await client.PostAsJsonAsync("/api/v1/progress", new { slug, lastBlockOrder = 2 });
        await client.PostAsJsonAsync("/api/v1/progress", new { slug, lastBlockOrder = 3 });

        var home = await DataOf(await client.GetAsync("/api/v1/home"));

        // Three reads, one day, one day of streak. The day comes from the SERVER's clock: accept it from the
        // request and the streak is a number anybody keeps alive by lying about what day it is — which is the
        // whole mechanic, for free, and the reason `09` banned gamification in the first place.
        Assert.Equal(1, home.GetProperty("streak").GetProperty("current").GetInt32());
        Assert.Equal(1, home.GetProperty("streak").GetProperty("longest").GetInt32());
    }

    [Fact]
    public async Task The_home_offers_to_continue_the_unfinished_topic_and_stops_offering_once_it_is_done()
    {
        var (client, _) = await ReaderAsync();
        var slug = await SeedAsync(blocks: 8);

        await client.PostAsJsonAsync("/api/v1/progress", new { slug, lastBlockOrder = 3 });

        var home = await DataOf(await client.GetAsync("/api/v1/home"));
        var resume = home.GetProperty("continue");

        Assert.Equal(slug, resume.GetProperty("slug").GetString());
        Assert.Equal(3, resume.GetProperty("lastBlockOrder").GetInt32());
        Assert.Equal(8, resume.GetProperty("totalBlocks").GetInt32());

        await client.PostAsJsonAsync("/api/v1/progress", new { slug, lastBlockOrder = 8, completed = true });

        // Offering to continue something they finished would be the home screen sending them backwards. With
        // nothing else unfinished, the honest answer is null — and the UI says so rather than faking a card.
        var after = await DataOf(await client.GetAsync("/api/v1/home"));
        Assert.Equal(JsonValueKind.Null, after.GetProperty("continue").ValueKind);
    }

    [Fact]
    public async Task A_reader_who_has_read_nothing_gets_an_honest_empty_home_not_an_error()
    {
        var (client, _) = await ReaderAsync();

        var response = await client.GetAsync("/api/v1/home");

        response.EnsureSuccessStatusCode();

        var home = await DataOf(response);

        // The first screen a new account ever sees. It has no streak, no place to continue and no history —
        // and every one of those is a normal state, not a 404 and not a crash on a null.
        Assert.Equal(0, home.GetProperty("streak").GetProperty("current").GetInt32());
        Assert.Equal(JsonValueKind.Null, home.GetProperty("continue").ValueKind);
        Assert.True(home.GetProperty("levels").GetArrayLength() > 0);
    }

    [Fact]
    public async Task One_readers_progress_is_not_another_readers()
    {
        var (mine, _) = await ReaderAsync();
        var (theirs, _) = await ReaderAsync();
        var slug = await SeedAsync(blocks: 5);

        await mine.PostAsJsonAsync("/api/v1/progress", new { slug, lastBlockOrder = 4 });

        var home = await DataOf(await theirs.GetAsync("/api/v1/home"));

        // The row is keyed by the token's user id, not by anything in the body. If this ever fails, two
        // readers are sharing a place in a book.
        Assert.Equal(JsonValueKind.Null, home.GetProperty("continue").ValueKind);
        Assert.Equal(0, home.GetProperty("streak").GetProperty("current").GetInt32());
    }

    // ── helpers ────────────────────────────────────────────────────────────────────────────────────

    private static async Task<JsonElement> DataOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");

    /// <summary>
    /// A real user row plus a token minted by the real issuer.
    /// </summary>
    /// <remarks>
    /// The row has to exist: UserTopicProgress -> User is a real FK, so a token for a user who was never
    /// saved would fail on the constraint rather than on what the test is about.
    /// </remarks>
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

    /// <summary>One published topic with a known number of blocks — the number the server must clamp to.</summary>
    private async Task<string> SeedAsync(int blocks, ContentStatus status = ContentStatus.Published)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WhyStackDbContext>();

        var key = $"apitest.{Guid.CreateVersion7():N}";
        var slug = key.Replace('.', '-');

        var topic = new Topic
        {
            Id = Guid.CreateVersion7(),
            StableKey = key,
            Slug = slug,
            LineId = DeterministicId.For("line:b1-language-runtime"),
            Category = TopicCategory.Concept,
            Archetype = Archetype.Concept,
            DefaultLevel = SkillLevel.Junior,
            DefaultTitle = "What is X?",
            CreatedAtUtc = DateTime.UtcNow,
        };

        var version = new TopicVersion
        {
            Id = Guid.CreateVersion7(),
            TopicId = topic.Id,
            VersionNumber = 1,
            Status = status,
            CanonicalLanguageCode = "en",
            EstimatedReadingMinutes = 6,
            LastReviewedOn = new DateOnly(2026, 7, 14),
            CreatedAtUtc = DateTime.UtcNow,
        };

        version.Translations.Add(new TopicTranslation
        {
            Id = Guid.CreateVersion7(),
            TopicVersionId = version.Id,
            LanguageCode = "en",
            Title = "What is X?",
            Status = TranslationStatus.HumanDraft,
            CreatedAtUtc = DateTime.UtcNow,
        });

        for (var order = 1; order <= blocks; order++)
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
