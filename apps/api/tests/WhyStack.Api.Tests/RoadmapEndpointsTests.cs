using System.Net;
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
/// The line map (the design's "Yol Haritan — Backend Hattı").
/// </summary>
/// <remarks>
/// The rule this file mostly exists to nail down is a NEGATIVE one: <b>"Ahead" is not "locked".</b> The
/// design dims a station the reader has not reached; the product does not impose an order. Those are one
/// keystroke apart in the UI and a completely different promise, and the only way "dimmed" quietly becomes
/// "forbidden" is if nothing ever asserts otherwise.
/// </remarks>
public class RoadmapEndpointsTests(WhyStackApiFactory factory) : IClassFixture<WhyStackApiFactory>
{
    [Fact]
    public async Task A_dimmed_station_is_still_a_station_you_can_walk_into()
    {
        var (client, _) = await ReaderAsync();
        var lineKey = await LineAsync();

        // A Junior station exists, so the Expert one cannot be the reader's next stop by default. Without
        // this the line has one station, that station is "Next", and the test would be asserting nothing.
        await SeedAsync(lineKey, SkillLevel.Junior, title: "Aaa — the near one");
        var slug = await SeedAsync(lineKey, SkillLevel.Expert, title: "Zzz — the far one");

        var station = Station(await MapAsync(client, lineKey), slug);

        // Dimmed in the design, and the furthest thing from the reader on the line.
        Assert.Equal("Ahead", station.GetProperty("state").GetString());

        var read = await client.GetAsync($"/api/v1/topics/{slug}");

        // THE ASSERTION THIS FILE EXISTS FOR.
        //
        // The reader opens it anyway, and the server hands it over. "Sıra dayatmıyoruz" is a promise, and a
        // promise with no test is a preference. Somebody adding a prerequisite check to the topic endpoint
        // has to walk past this failure to do it.
        read.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task The_line_runs_junior_to_expert()
    {
        var (client, _) = await ReaderAsync();
        var lineKey = await LineAsync();

        var expert = await SeedAsync(lineKey, SkillLevel.Expert, title: "Zzz — expert");
        var junior = await SeedAsync(lineKey, SkillLevel.Junior, title: "Aaa — junior");

        var stations = (await MapAsync(client, lineKey)).GetProperty("stations").EnumerateArray()
            .Select(station => station.GetProperty("slug").GetString())
            .ToList();

        // Level first, and it must beat the alphabet: seeded so that sorting by title alone would put the
        // Expert topic second and look correct by accident. The basamak is the spine of the product — a line
        // that runs Expert -> Junior is not a roadmap, it is a list.
        Assert.Equal([junior, expert], stations);
    }

    [Fact]
    public async Task Where_you_stopped_is_where_you_are_and_what_you_never_opened_is_next()
    {
        var (client, _) = await ReaderAsync();
        var lineKey = await LineAsync();

        var started = await SeedAsync(lineKey, SkillLevel.Junior, blocks: 10, title: "Aaa — started");
        var untouched = await SeedAsync(lineKey, SkillLevel.Junior, blocks: 10, title: "Bbb — untouched");

        await client.PostAsJsonAsync("/api/v1/progress", new { slug = started, lastBlockOrder = 4 });

        var line = await MapAsync(client, lineKey);

        Assert.Equal("Current", Station(line, started).GetProperty("state").GetString());
        Assert.Equal(40, Station(line, started).GetProperty("percent").GetInt32());

        // "Next" is the nearest station never opened — a suggestion the design draws as the next stop. It is
        // not a gate, and nothing about it is enforced anywhere.
        Assert.Equal("Next", Station(line, untouched).GetProperty("state").GetString());
        Assert.Equal(0, Station(line, untouched).GetProperty("percent").GetInt32());
    }

    [Fact]
    public async Task A_finished_station_reads_a_hundred_percent_and_stops_being_current()
    {
        var (client, _) = await ReaderAsync();
        var lineKey = await LineAsync();
        var slug = await SeedAsync(lineKey, SkillLevel.Junior, blocks: 7);

        await client.PostAsJsonAsync(
            "/api/v1/progress",
            new { slug, lastBlockOrder = 7, completed = true });

        var station = Station(await MapAsync(client, lineKey), slug);

        Assert.Equal("Done", station.GetProperty("state").GetString());
        Assert.Equal(100, station.GetProperty("percent").GetInt32());
    }

    [Fact]
    public async Task An_ecosystem_nobody_has_is_refused_rather_than_drawn_empty()
    {
        var (client, _) = await ReaderAsync();

        var response = await client.GetAsync("/api/v1/roadmap?ecosystem=dotnett&line=b1-language-runtime&language=en");

        // An empty map and "no such ecosystem" look identical to a reader, and only one of them is worth
        // telling them about. Same for a domain that does not exist.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync("/api/v1/roadmap?ecosystem=dotnet&line=nosuchline")).StatusCode);
    }

    [Fact]
    public async Task Asking_for_a_line_without_saying_which_is_a_422()
    {
        var (client, _) = await ReaderAsync();

        Assert.Equal(
            HttpStatusCode.UnprocessableEntity,
            (await client.GetAsync("/api/v1/roadmap?line=b1-language-runtime")).StatusCode);
    }

    /// <summary>
    /// The tabs belong to an AREA (ADR-0027): Backend's ecosystems are languages, Frontend's are frameworks.
    /// A flat list would offer .NET on the Frontend tab strip.
    /// </summary>
    [Fact]
    public async Task The_tabs_show_the_ecosystems_we_have_not_written_yet()
    {
        var (client, _) = await ReaderAsync();

        var lines = (await DataOf(await client.GetAsync("/api/v1/areas/backend/ecosystems"))).EnumerateArray().ToList();

        Assert.Contains(lines, line => line.GetProperty("key").GetString() == "dotnet");

        // The unavailable ones are RETURNED, greyed with "YAKINDA" rather than hidden. Filtering them out
        // here would make the product look like it thinks .NET is all there is — and the promise a reader
        // can see is worth more than the shorter list.
        Assert.Contains(lines, line => !line.GetProperty("isAvailable").GetBoolean());
    }

    [Fact]
    public async Task A_line_belongs_to_the_reader_asking_for_it()
    {
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await factory.CreateClient().GetAsync("/api/v1/roadmap?ecosystem=dotnet&line=b1-language-runtime")).StatusCode);
    }

    // ── helpers ────────────────────────────────────────────────────────────────────────────────────

    private static async Task<JsonElement> DataOf(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
    }

    /// <summary>The map for one line. Named Map, not Line, so it cannot be confused with the fixture below.</summary>
    private static async Task<JsonElement> MapAsync(HttpClient client, string line) =>
        await DataOf(await client.GetAsync($"/api/v1/roadmap?ecosystem=dotnet&line={line}&language=en"));

    /// <summary>
    /// A line of this test's own, so the map contains this test's stations and nothing else.
    /// </summary>
    /// <remarks>
    /// Not fussiness. "Next" is the first UNSTARTED station on the whole line — a fact about the line, not
    /// about a topic — so a sibling test seeding one more Backend topic silently moves it. The first version
    /// shared `backend` and failed depending on which test ran first, which is the kind of red that teaches
    /// you nothing.
    /// </remarks>
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

    private static JsonElement Station(JsonElement line, string slug) =>
        Assert.Single(
            line.GetProperty("stations").EnumerateArray(),
            station => station.GetProperty("slug").GetString() == slug);

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

    private async Task<string> SeedAsync(string line, SkillLevel level, int blocks = 5, string? title = null)
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
            LineId = await LineIdAsync(line),
            Category = TopicCategory.Concept,
            Archetype = Archetype.Concept,
            DefaultLevel = level,
            DefaultTitle = title ?? "A station",
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
        };

        version.Translations.Add(new TopicTranslation
        {
            Id = Guid.CreateVersion7(),
            TopicVersionId = version.Id,
            LanguageCode = "en",
            Title = title ?? "A station",
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

    private async Task<Guid> LineIdAsync(string key)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WhyStackDbContext>();

        return await context.Lines.Where(line => line.Key == key).Select(line => line.Id).SingleAsync();
    }
}
