using WhyStack.Application.Content.Authoring;
using WhyStack.Application.Content.Blocks;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Infrastructure.Tests;

/// <summary>
/// The block write path (ADR-0024), against a real SQL Server.
/// </summary>
/// <remarks>
/// Every test runs in a transaction that is never committed, so the shared dev database is untouched.
/// </remarks>
[Collection(DatabaseCollection.Name)]
public sealed class BlockAuthoringTests(DatabaseFixture fixture)
{
    private static SaveTopicCommand Topic(string suffix, IReadOnlyList<BlockCommand> blocks) => new(
        Id: null,
        StableKey: $"backend.block-test-{suffix}",
        Slug: $"block-test-{suffix}",
        LineKey: "b1-language-runtime",
        ScopeKey: null,
        Category: "Concept",
        Archetype: "Mechanism",
        Level: "MidLevel",
        EstimatedReadingMinutes: 8,
        SupportedVersions: [],
        Translations: [new TranslationCommand("en", "Block Test", "A fixture topic.")],
        Blocks: blocks,
        Sections: [],
        Implementations: [],
        Relationships: [],
        RowVersion: null);

    private static BlockCommand Block(int order, string type, object payload, string? ecosystem = null) =>
        new(order, type, "en", ecosystem, BlockData.Serialize(payload));

    /// <summary>A block flow survives the round trip: order, type, ecosystem tag and body.</summary>
    [Fact]
    public async Task A_block_flow_is_saved_and_read_back_intact()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);

        var command = Topic("roundtrip",
        [
            Block(1, "Hook", new HookData("Where is the thread during an awaited call?", "You will know in 8 minutes.")),
            Block(2, "Concept", new ProseData("A thread is not waiting; a continuation is.")),
            Block(3, "Code", new CodeData("csharp", "await Foo();", File: "S.cs"), ecosystem: "dotnet"),
            Block(4, "Checkpoint", new CheckpointData("Pick one.", ["blocks", "returns to the pool"], 1, "It is freed.")),
            Block(5, "Summary", new SummaryData(["await does not wait."])),
            Block(6, "Next", new NextData("ConfigureAwait")),
        ]);

        var outcome = await repository.SaveAsync(command, Guid.CreateVersion7(), default);

        Assert.Null(outcome.Error);

        var editable = await repository.EditableAsync(outcome.Id, default);

        Assert.Equal(6, editable!.Blocks.Count);
        Assert.Equal(["Hook", "Concept", "Code", "Checkpoint", "Summary", "Next"], editable.Blocks.Select(b => b.Type));

        // The ecosystem tag is what makes "shared why + per-ecosystem treatment" work at all (ADR-0024).
        Assert.Null(editable.Blocks.Single(b => b.Type == "Hook").EcosystemKey);
        Assert.Equal("dotnet", editable.Blocks.Single(b => b.Type == "Code").EcosystemKey);

        Assert.Equal("Mechanism", editable.Archetype);
        Assert.Contains("awaited call", editable.Blocks.Single(b => b.Type == "Hook").DataJson);
    }

    /// <summary>
    /// Saving REPLACES the flow. A block the editor deleted must be gone, not orphaned in the middle of it.
    /// </summary>
    [Fact]
    public async Task Saving_replaces_the_flow_rather_than_merging_into_it()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);

        var first = await repository.SaveAsync(
            Topic("replace",
            [
                Block(1, "Hook", new HookData("First?")),
                Block(2, "Concept", new ProseData("Kept.")),
                Block(3, "Myth", new MythData("Assumed", "Actually")),
            ]),
            Guid.CreateVersion7(),
            default);

        var reload = await repository.EditableAsync(first.Id, default);

        // The editor deleted the myth and rewrote the hook.
        await repository.SaveAsync(
            Topic("replace", [Block(1, "Hook", new HookData("Rewritten?"))]) with
            {
                Id = first.Id,
                RowVersion = reload!.RowVersion,
            },
            Guid.CreateVersion7(),
            default);

        var after = await repository.EditableAsync(first.Id, default);

        Assert.Single(after!.Blocks);
        Assert.Contains("Rewritten", after.Blocks[0].DataJson);
        Assert.DoesNotContain(after.Blocks, block => block.Type == "Myth");
    }

    /// <summary>An archetype nobody defined is a clean 422, not an Enum.Parse throwing three layers up.</summary>
    [Fact]
    public async Task An_unknown_archetype_is_refused_on_the_archetype_field()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);

        var outcome = await repository.SaveAsync(
            Topic("badarch", []) with { Archetype = "Interpretive-Dance" },
            Guid.CreateVersion7(),
            default);

        Assert.NotNull(outcome.Error);
        Assert.Equal("archetype", outcome.ErrorField);
    }
}
