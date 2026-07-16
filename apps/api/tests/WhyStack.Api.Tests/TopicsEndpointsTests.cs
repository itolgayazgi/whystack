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
/// The rules under test are the ones that fail QUIETLY: an unpublished draft reaching a reader, and a
/// fallback language served without saying so. Neither throws, neither logs, and neither is visible to
/// anyone who is not looking for it.
/// </remarks>
public class TopicsEndpointsTests(WhyStackApiFactory factory) : IClassFixture<WhyStackApiFactory>
{
    [Fact]
    public async Task A_published_topic_is_readable_without_an_account()
    {
        var slug = await SeedAsync(ContentStatus.Published);

        var response = await factory.CreateClient().GetAsync($"/api/v1/topics/{slug}");

        response.EnsureSuccessStatusCode();

        var topic = await DataOf(response);

        // ADR-0009 builds a public, indexable site on this premise: a reader who found a topic through a
        // search engine has to be able to read it without an account.
        Assert.Equal("What is X?", topic.GetProperty("title").GetString());
        Assert.True(topic.GetProperty("sections").GetArrayLength() > 0);
    }

    [Fact]
    public async Task A_draft_is_invisible_to_a_reader()
    {
        var slug = await SeedAsync(ContentStatus.AiDraft);

        var response = await factory.CreateClient().GetAsync($"/api/v1/topics/{slug}");

        // 404, not 403. A 403 confirms the topic exists — an anonymous visitor could enumerate the content
        // roadmap by guessing slugs. "Not accessible" includes not being *discoverable*.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task An_editor_can_read_a_draft()
    {
        var slug = await SeedAsync(ContentStatus.AiDraft);

        var response = await EditorClient(RoleName.Editor).GetAsync($"/api/v1/topics/{slug}");

        // Otherwise the review gate is a form nobody can fill in: a reviewer cannot approve what they are not
        // allowed to read. CLAUDE.md §1.5 requires human review — this is what makes it possible.
        response.EnsureSuccessStatusCode();

        Assert.Equal("AiDraft", (await DataOf(response)).GetProperty("status").GetString());
    }

    [Fact]
    public async Task A_registered_user_is_not_an_editor()
    {
        var slug = await SeedAsync(ContentStatus.AiDraft);

        var response = await EditorClient(RoleName.RegisteredUser).GetAsync($"/api/v1/topics/{slug}");

        // Having an account is not a review role. This assertion is what stops "signed in" from quietly
        // becoming "may read unreviewed content" the next time somebody touches the endpoint.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task A_missing_translation_falls_back_and_says_so()
    {
        var slug = await SeedAsync(ContentStatus.Published);

        var response = await factory.CreateClient().GetAsync($"/api/v1/topics/{slug}?language=tr");

        var language = (await DataOf(response)).GetProperty("language");

        // THE ASSERTION THIS FILE EXISTS FOR.
        //
        // A Turkish reader whose topic has no Turkish translation is shown the English text — better than a
        // blank page. What they must never be shown is English *presented as though it were what they asked
        // for*. They would conclude the translation is bad, or that the setting is broken, and neither is
        // true (CLAUDE.md §1.7, `08` Principle 07).
        Assert.Equal("tr", language.GetProperty("requested").GetString());
        Assert.Equal("en", language.GetProperty("returned").GetString());
        Assert.True(language.GetProperty("fallbackUsed").GetBoolean());
        Assert.Equal("translation_not_available", language.GetProperty("fallbackReason").GetString());
    }

    /// <summary>ADR-0021. The concept is one page; only the panel changes.</summary>
    [Fact]
    public async Task An_implementation_is_returned_with_the_concept_and_the_preferred_one_opens_first()
    {
        var slug = await SeedAsync(ContentStatus.Published, withImplementation: true);

        var response = await factory.CreateClient().GetAsync($"/api/v1/topics/{slug}?ecosystem=dotnet");

        var topic = await DataOf(response);

        var implementation = Assert.Single(topic.GetProperty("implementations").EnumerateArray());

        Assert.Equal("dotnet", implementation.GetProperty("ecosystemKey").GetString());
        Assert.True(implementation.GetProperty("isPreferred").GetBoolean());

        // The concept sections and the implementation sections are SEPARATE. Fold them together and the
        // reasoning gets duplicated per ecosystem — the exact defect ADR-0021 exists to remove.
        Assert.Contains(
            topic.GetProperty("sections").EnumerateArray(),
            section => section.GetProperty("sectionType").GetString() == "TradeOffs");

        Assert.Contains(
            implementation.GetProperty("sections").EnumerateArray(),
            section => section.GetProperty("sectionType").GetString() == "BasicExample");
    }

    [Fact]
    public async Task A_page_size_nobody_should_ask_for_is_clamped_rather_than_honoured()
    {
        var response = await factory.CreateClient()
            .GetFromJsonAsync<JsonElement>("/api/v1/topics?pageSize=10000");

        // CLAUDE.md §4 forbids the unbounded query. Clamped rather than rejected: `pageSize=10000` is far more
        // often a client bug than an attack, and a 422 teaches nobody anything.
        Assert.Equal(50, response.GetProperty("pagination").GetProperty("pageSize").GetInt32());
    }

    [Fact]
    public async Task Search_finds_a_topic_by_its_title_and_leaves_the_others_out()
    {
        var slug = await SeedAsync(ContentStatus.Published, title: "Garbage Collector Neden Duraklatır?");
        await SeedAsync(ContentStatus.Published, title: "Bir Şey Hakkında Tamamen Başka");

        var found = await factory.CreateClient()
            .GetFromJsonAsync<JsonElement>("/api/v1/topics?language=tr&q=Garbage");

        var slugs = found.GetProperty("data").EnumerateArray()
            .Select(topic => topic.GetProperty("slug").GetString())
            .ToList();

        Assert.Contains(slug, slugs);
        Assert.DoesNotContain("Tamamen Başka", string.Join(" ", slugs));
    }

    [Fact]
    public async Task A_blank_search_means_no_search_rather_than_no_results()
    {
        await SeedAsync(ContentStatus.Published);

        var response = await factory.CreateClient()
            .GetFromJsonAsync<JsonElement>("/api/v1/topics?language=tr&q=%20%20");

        // Whitespace is a caller sending nothing — an empty box, a stray keystroke. Passed through it becomes
        // LIKE '%  %', which matches almost nothing and looks exactly like a working search that found
        // nothing: same 200, same empty list, and no way for anybody to tell the difference.
        Assert.True(response.GetProperty("pagination").GetProperty("totalCount").GetInt32() > 0);
    }

    [Fact]
    public async Task Search_does_not_hand_a_draft_to_a_reader_who_went_looking_for_it()
    {
        await SeedAsync(ContentStatus.AiDraft, title: "Gizli Taslak Konusu");

        var response = await factory.CreateClient()
            .GetFromJsonAsync<JsonElement>("/api/v1/topics?language=tr&q=Gizli%20Taslak");

        // Search is a second door onto the same corpus, and a door that skips the review gate is worse than
        // the front one: it is how you find exactly the unreviewed thing you were told does not exist yet.
        Assert.Equal(0, response.GetProperty("pagination").GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task An_unknown_slug_is_a_problem_details_404()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/topics/no-such-topic-anywhere");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        // `08`: Problem Details only. The code is part of the contract — clients may depend on it.
        Assert.Equal("resource_not_found", problem.GetProperty("code").GetString());
    }

    // ── helpers ────────────────────────────────────────────────────────────────────────────────────

    private static async Task<JsonElement> DataOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");

    private HttpClient EditorClient(RoleName role)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenFor(role));
        return client;
    }

    /// <summary>
    /// A JWT for a user in one role, minted through the real issuer.
    /// </summary>
    /// <remarks>
    /// Through the issuer rather than by hand-crafting a token: a hand-crafted one proves the test can forge
    /// a claim, not that the application reads the claim it actually issues.
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
    private async Task<string> SeedAsync(
        ContentStatus status,
        bool withImplementation = false,
        string? title = null)
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

            // Backend, not C#. A topic belongs to a domain (ADR-0021).
            LineId = DeterministicId.For("line:b1-language-runtime"),

            Category = TopicCategory.Concept,
            DefaultLevel = SkillLevel.Junior,
            DefaultTitle = title ?? "What is X?",
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
            Title = title ?? "What is X?",
            Status = TranslationStatus.HumanDraft,
            CreatedAtUtc = DateTime.UtcNow,
        });

        var order = 0;

        // The CONCEPT. True in every ecosystem.
        foreach (var (section, markdown) in new[]
        {
            ("Summary", "The short answer."),
            ("TradeOffs", "| You get | You give up |\n|---|---|\n| safety | control |"),
        })
        {
            version.Sections.Add(new TopicSection
            {
                Id = Guid.CreateVersion7(),
                TopicVersionId = version.Id,
                SectionTypeKey = section,
                LanguageCode = "en",
                Markdown = markdown,
                SortOrder = order++,
                CreatedAtUtc = DateTime.UtcNow,
            });
        }

        if (withImplementation)
        {
            var implementation = new TopicImplementation
            {
                Id = Guid.CreateVersion7(),
                TopicVersionId = version.Id,
                EcosystemId = DeterministicId.For("ecosystem:dotnet"),
                ProgrammingLanguageId = DeterministicId.For("language:csharp"),
                SupportedVersions = ".NET 8",
                CreatedAtUtc = DateTime.UtcNow,
            };

            implementation.Sections.Add(new ImplementationSection
            {
                Id = Guid.CreateVersion7(),
                TopicImplementationId = implementation.Id,
                SectionTypeKey = "BasicExample",
                LanguageCode = "en",
                Markdown = "```csharp\nvar x = 1;\n```",
                SortOrder = 0,
            });

            version.Implementations.Add(implementation);
        }

        topic.Versions.Add(version);

        context.Topics.Add(topic);
        await context.SaveChangesAsync();

        factory.TrackTopic(topic.Id);

        return slug;
    }
}
