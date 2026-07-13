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
/// The topic endpoints, against the real application and the real database.
/// </summary>
/// <remarks>
/// The rules under test are the ones that would fail quietly: an unpublished draft reaching a reader, and
/// a fallback language served without saying so. Neither throws, neither logs, and neither is visible to
/// anyone who is not looking for it — which is exactly why they are asserted here rather than trusted.
/// </remarks>
public class TopicsEndpointsTests(WhyStackApiFactory factory) : IClassFixture<WhyStackApiFactory>
{
    /// <summary>A real file, so the content reader has real Markdown to parse. It is also the shipped topic.</summary>
    private const string RealMarkdown = "content/topics/csharp/what-is-csharp/en.md";

    private const string RealTurkishMarkdown = "content/topics/csharp/what-is-csharp/tr.md";

    [Fact]
    public async Task A_published_topic_is_readable_without_an_account()
    {
        var (_, slug) = await SeedAsync(ContentStatus.Published);

        var response = await factory.CreateClient().GetAsync($"/api/v1/topics/{slug}");

        response.EnsureSuccessStatusCode();

        var topic = await DataOf(response);

        Assert.Equal("What is C#?", topic.GetProperty("title").GetString());

        // ADR-0009 builds a public, indexable site on this premise: a reader who found a topic through a
        // search engine has to be able to read it without an account.
        Assert.True(topic.GetProperty("sections").GetArrayLength() > 0);
    }

    [Fact]
    public async Task A_draft_is_invisible_to_a_reader()
    {
        var (_, slug) = await SeedAsync(ContentStatus.AiDraft);

        var response = await factory.CreateClient().GetAsync($"/api/v1/topics/{slug}");

        // 404, not 403. A 403 would confirm the topic exists — an anonymous visitor could enumerate the
        // content roadmap by guessing slugs. `04` says draft content is not publicly accessible, and "not
        // accessible" includes not being *discoverable*.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var list = await factory.CreateClient().GetFromJsonAsync<JsonElement>("/api/v1/topics?pageSize=50");

        Assert.DoesNotContain(
            list.GetProperty("data").EnumerateArray(),
            item => item.GetProperty("slug").GetString() == slug);
    }

    [Fact]
    public async Task An_editor_can_read_a_draft()
    {
        var (_, slug) = await SeedAsync(ContentStatus.AiDraft);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenFor(RoleName.Editor));

        var response = await client.GetAsync($"/api/v1/topics/{slug}");

        // Otherwise the review gate is a form nobody can fill in: a reviewer cannot approve what they are
        // not allowed to read. CLAUDE.md §1.5 requires human review — this is what makes it possible.
        response.EnsureSuccessStatusCode();

        Assert.Equal("AiDraft", (await DataOf(response)).GetProperty("status").GetString());
    }

    [Fact]
    public async Task A_registered_user_is_not_an_editor()
    {
        var (_, slug) = await SeedAsync(ContentStatus.AiDraft);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenFor(RoleName.RegisteredUser));

        // Having an account is not a review role. This is the assertion that stops "signed in" from
        // quietly becoming "may read unreviewed content" the next time somebody touches the endpoint.
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/topics/{slug}")).StatusCode);
    }

    [Fact]
    public async Task A_translation_is_served_in_the_language_that_was_asked_for()
    {
        var (_, slug) = await SeedAsync(ContentStatus.Published, withTurkish: true);

        var response = await factory.CreateClient().GetAsync($"/api/v1/topics/{slug}?language=tr");

        var topic = await DataOf(response);

        Assert.Equal("C# Nedir?", topic.GetProperty("title").GetString());
        Assert.False(topic.GetProperty("language").GetProperty("fallbackUsed").GetBoolean());
    }

    [Fact]
    public async Task A_missing_translation_falls_back_and_says_so()
    {
        var (_, slug) = await SeedAsync(ContentStatus.Published, withTurkish: false);

        var response = await factory.CreateClient().GetAsync($"/api/v1/topics/{slug}?language=tr");

        var language = (await DataOf(response)).GetProperty("language");

        // THE ASSERTION THIS FILE EXISTS FOR.
        //
        // A Turkish reader whose topic has no Turkish translation is shown the English text — that is
        // better than a blank page. What they must never be shown is English *presented as though it were
        // what they asked for*. They would conclude the translation is bad, or that the setting is broken,
        // and neither is true (CLAUDE.md §1.7, `08` Principle 07 — No Silent Fallbacks).
        Assert.Equal("tr", language.GetProperty("requested").GetString());
        Assert.Equal("en", language.GetProperty("returned").GetString());
        Assert.True(language.GetProperty("fallbackUsed").GetBoolean());
        Assert.Equal("translation_not_available", language.GetProperty("fallbackReason").GetString());
    }

    [Fact]
    public async Task A_page_size_nobody_should_ask_for_is_clamped_rather_than_honoured()
    {
        var response = await factory.CreateClient()
            .GetFromJsonAsync<JsonElement>("/api/v1/topics?pageSize=10000");

        // CLAUDE.md §4 forbids the unbounded query. Clamped rather than rejected: `pageSize=10000` is far
        // more often a client bug than an attack, and a 422 teaches nobody anything — while the metadata
        // says exactly what was served.
        Assert.Equal(50, response.GetProperty("pagination").GetProperty("pageSize").GetInt32());
    }

    [Fact]
    public async Task An_unknown_slug_is_a_problem_details_404()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/topics/no-such-topic-anywhere");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        // `08`: Problem Details only. A custom error shape is forbidden, and the code is part of the
        // contract — clients may depend on it.
        Assert.Equal("resource_not_found", problem.GetProperty("code").GetString());
    }

    // ── helpers ────────────────────────────────────────────────────────────────────────────────────

    private static async Task<JsonElement> DataOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");

    /// <summary>
    /// A JWT for a user in one role, minted through the real issuer.
    /// </summary>
    /// <remarks>
    /// Through the issuer rather than by hand-crafting a token: a hand-crafted one proves the test can
    /// forge a claim, not that the application reads the claim it actually issues.
    /// </remarks>
    private string TokenFor(RoleName role)
    {
        using var scope = factory.Services.CreateScope();
        var issuer = scope.ServiceProvider.GetRequiredService<IAccessTokenIssuer>();

        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = $"{Guid.CreateVersion7():N}@example.com",
            NormalizedEmail = $"{Guid.CreateVersion7():N}@EXAMPLE.COM",
            DisplayName = "Test",
            PasswordHash = "irrelevant",
        };

        return issuer.Issue(user, [role]).Token;
    }

    /// <summary>Seeds one topic with a unique identity — these tests share a database with everything else.</summary>
    private async Task<(Guid Id, string Slug)> SeedAsync(ContentStatus status, bool withTurkish = false)
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
            Technology = "csharp",
            Category = TopicCategory.Concept,
            DefaultLevel = SkillLevel.Junior,
            DefaultTitle = "What is C#?",
            CreatedAtUtc = DateTime.UtcNow,
        };

        var version = new TopicVersion
        {
            Id = Guid.CreateVersion7(),
            TopicId = topic.Id,
            VersionNumber = 1,
            Status = status,
            CanonicalLanguageCode = "en",
            MarkdownPath = RealMarkdown,
            ContentHash = Guid.CreateVersion7().ToString("N"),
            EstimatedReadingMinutes = 6,
            LastReviewedOn = new DateOnly(2026, 7, 13),
            CreatedAtUtc = DateTime.UtcNow,
        };

        // Real section keys, in blueprint order, so the content reader has something to find in the file.
        var order = 0;
        string[] blueprint = ["Summary", "Definition", "CoreMentalModel", "TradeOffs"];

        foreach (var section in blueprint)
        {
            version.Sections.Add(new TopicSection
            {
                Id = Guid.CreateVersion7(),
                TopicVersionId = version.Id,
                SectionTypeKey = section,
                SortOrder = order++,
            });
        }

        if (withTurkish)
        {
            version.Translations.Add(new TopicTranslation
            {
                Id = Guid.CreateVersion7(),
                TopicVersionId = version.Id,
                LanguageCode = "tr",
                Title = "C# Nedir?",
                MarkdownPath = RealTurkishMarkdown,
                ContentHash = Guid.CreateVersion7().ToString("N"),
                Status = TranslationStatus.MachineDraft,
                CreatedAtUtc = DateTime.UtcNow,
            });
        }

        topic.Versions.Add(version);

        context.Topics.Add(topic);
        await context.SaveChangesAsync();

        return (topic.Id, slug);
    }
}
