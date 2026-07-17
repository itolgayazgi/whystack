using Microsoft.EntityFrameworkCore;
using WhyStack.Application.Content;
using WhyStack.Application.Content.Authoring;
using WhyStack.Domain.Content;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Infrastructure.Tests;

/// <summary>
/// A reader who chose no ecosystem still gets the topic.
/// </summary>
/// <remarks>
/// <b>This file exists because the first topic WhyStack ever published rendered "bu konunun içeriği henüz
/// yazılmadı" over six blocks that were written.</b>
///
/// Every block was tagged `dotnet`, correctly — "C# neden var?" is .NET's question. The reader sends no
/// ecosystem until somebody picks one, and the merge drops what is tagged for another:
/// <c>EcosystemKey is null || EcosystemKey == ecosystem</c>. With `ecosystem` null, all six went. And the
/// switch that would have let somebody pick .NET was drawn from <c>implementations</c> — the model ADR-0024
/// retired, of which a block topic has none — so it never rendered. The content was unreachable by any click,
/// and the page said it did not exist.
///
/// The rule now: a caller who names an ecosystem gets it; a caller who names none gets the FIRST, and the
/// response says which. A visitor from a search engine has chosen no ecosystem and never will.
/// </remarks>
[Collection(DatabaseCollection.Name)]
public sealed class ReaderEcosystemTests(DatabaseFixture fixture)
{
    private const string TestLine = "b1-language-runtime";

    private static BlockCommand Block(string type, int order, string? ecosystem) =>
        new(order, type, "en", ecosystem, Body(type));

    private static string Body(string type) => type switch
    {
        "Hook" => """{"question":"Why?"}""",
        "Checkpoint" => """{"question":"Which?","options":["A","B"],"correctIndex":0,"explanation":"A."}""",
        "Summary" => """{"items":["It exists."]}""",
        _ => """{"label":"Next"}""",
    };

    /// <summary>The four beats, all tagged for one ecosystem — exactly the shape that broke.</summary>
    private static SaveTopicCommand Topic(string suffix, string? ecosystem) => new(
        Id: null,
        StableKey: $"backend.eco-test-{suffix}",
        Slug: $"eco-test-{suffix}",
        LineKey: TestLine,
        ScopeKey: null,
        Category: "Concept",
        Archetype: "Concept",
        Sequence: null,
        Level: "Junior",
        EstimatedReadingMinutes: 6,
        SupportedVersions: [],
        Translations: [new TranslationCommand("en", "Eco fixture", "A fixture stop.")],
        Blocks:
        [
            Block("Hook", 1, ecosystem),
            Block("Checkpoint", 2, ecosystem),
            Block("Summary", 3, ecosystem),
            Block("Next", 4, ecosystem),
        ],
        Sections: [],
        Implementations: [],
        Relationships: [],
        RowVersion: null);

    private static async Task<string> PublishAsync(WhyStackDbContext context, SaveTopicCommand command)
    {
        var outcome = await new ContentAuthoringRepository(context, TimeProvider.System)
            .SaveAsync(command, Guid.CreateVersion7(), default);

        Assert.Null(outcome.Error);

        await context.TopicVersions
            .Where(version => version.TopicId == outcome.Id)
            .ExecuteUpdateAsync(set => set.SetProperty(version => version.Status, ContentStatus.Published));

        return command.Slug;
    }

    private static GetTopicHandler HandlerFor(WhyStackDbContext context) =>
        new(new TopicRepository(context));

    [Fact]
    public async Task A_reader_who_named_no_ecosystem_still_sees_the_blocks()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var slug = await PublishAsync(context, Topic("dotnet-only", "dotnet"));

        var result = await HandlerFor(context).HandleAsync(slug, "en", null, false, default);

        // THE ASSERTION THIS FILE EXISTS FOR. This was 0, and the page rendered "içeriği henüz yazılmadı".
        Assert.Equal(4, result.Value!.Blocks.Count);

        // And it SAYS which treatment it served, rather than serving one silently.
        var selected = Assert.Single(result.Value.Ecosystems, option => option.IsSelected);
        Assert.Equal("dotnet", selected.Key);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task The_switch_can_be_drawn_from_what_the_response_carries()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var slug = await PublishAsync(context, Topic("switchable", "dotnet"));

        var result = await HandlerFor(context).HandleAsync(slug, "en", null, false, default);

        // The blocks are filtered before they reach the client, so this list is the ONLY way it can know what
        // it is not being shown. The old switch read `implementations`, which a block topic never has.
        Assert.NotEmpty(result.Value!.Ecosystems);
        Assert.Empty(result.Value.Implementations);

        // The NAME, not the key: a reader picks ".NET", not "dotnet".
        Assert.Equal(".NET", result.Value.Ecosystems[0].Name);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task A_named_ecosystem_is_still_honoured()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var slug = await PublishAsync(context, Topic("named", "dotnet"));

        var result = await HandlerFor(context).HandleAsync(slug, "en", "dotnet", false, default);

        // The default only fills a silence. A caller who asks gets what they asked for.
        Assert.Equal(4, result.Value!.Blocks.Count);
        Assert.True(result.Value.Ecosystems.Single(option => option.Key == "dotnet").IsSelected);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task Asking_for_an_ecosystem_the_topic_does_not_have_shows_the_one_it_does()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var slug = await PublishAsync(context, Topic("wrong-eco", "dotnet"));

        var result = await HandlerFor(context).HandleAsync(slug, "en", "java", false, default);

        // A Java reader on a stop written for .NET is not an error — it is a reader this stop was not written
        // for. An empty page tells them nothing; the treatment that exists, labelled, tells them where they
        // are. IsSelected is what stops that being a silent swap.
        Assert.Equal(4, result.Value!.Blocks.Count);
        Assert.Equal("dotnet", result.Value.Ecosystems.Single(option => option.IsSelected).Key);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task A_topic_of_shared_blocks_offers_no_switch_and_reads_fine()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var slug = await PublishAsync(context, Topic("shared", null));

        var result = await HandlerFor(context).HandleAsync(slug, "en", null, false, default);

        // The "why" is true in every ecosystem (ADR-0021), so there is nothing to switch between. An empty
        // list here is the topic saying so — not a switch that failed to load.
        Assert.Equal(4, result.Value!.Blocks.Count);
        Assert.Empty(result.Value.Ecosystems);

        await transaction.RollbackAsync();
    }
}
