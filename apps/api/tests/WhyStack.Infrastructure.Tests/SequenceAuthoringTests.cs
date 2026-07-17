using WhyStack.Application.Content.Authoring;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Infrastructure.Tests;

/// <summary>
/// The chain badge — "OOP II / III" (ADR-0027).
/// </summary>
/// <remarks>
/// The schema has carried <c>TopicSequence</c> since ADR-0027 and the save path never did: an author could
/// not number a stop at all. This file covers the round trip and, more importantly, the shapes the type
/// system happily accepts and the reader cannot use.
///
/// <b>Why these are REFUSALS and not to-do items:</b> a missing Checkpoint is incomplete — the author has not
/// written it yet, and the problem list is the right answer. "OOP IV / III" is self-contradictory: there is no
/// part 4 of a 3-part chain, and no amount of further writing makes one. The deciding fact is that the publish
/// gate validates the draft's SECTIONS and nothing else, so a reported sequence problem would sit in a list
/// and publish anyway — which is precisely how the mandatory beats were documented as "the publish gate" while
/// being called by nobody.
///
/// Every test runs inside a transaction that is never committed.
/// </remarks>
[Collection(DatabaseCollection.Name)]
public sealed class SequenceAuthoringTests(DatabaseFixture fixture)
{
    private const string TestLine = "b1-language-runtime";

    private static SaveTopicCommand Topic(string suffix, SequenceCommand? sequence) => new(
        Id: null,
        StableKey: $"backend.sequence-test-{suffix}",
        Slug: $"sequence-test-{suffix}",
        LineKey: TestLine,
        ScopeKey: null,
        Category: "Concept",
        Archetype: "Concept",
        Sequence: sequence,
        Level: "Junior",
        EstimatedReadingMinutes: 6,
        SupportedVersions: [],
        Translations: [new TranslationCommand("en", "Sequence fixture", "A fixture stop.")],
        Blocks: [],
        Sections: [],
        Implementations: [],
        Relationships: [],
        RowVersion: null);

    private static SaveTopicHandler HandlerFor(WhyStackDbContext context) =>
        new(new ContentAuthoringRepository(context, TimeProvider.System), new TopicRepository(context));

    /// <summary>What Tolga is about to author: OOP I / II / III on B1 Junior.</summary>
    [Fact]
    public async Task A_numbered_stop_survives_the_round_trip()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);

        var outcome = await repository.SaveAsync(
            Topic("oop-2", new SequenceCommand("OOP", 2, 3)), Guid.CreateVersion7(), default);

        Assert.Null(outcome.Error);

        var editable = await repository.EditableAsync(outcome.Id, default);

        // Read back, not just written. A field the editor saves and cannot see again is a field that silently
        // resets to null on the next save — which is how "I set it three times and it never stuck" happens.
        Assert.NotNull(editable!.Sequence);
        Assert.Equal("OOP", editable.Sequence.Group);
        Assert.Equal(2, editable.Sequence.Part);
        Assert.Equal(3, editable.Sequence.Of);

        await transaction.RollbackAsync();
    }

    /// <summary>Most stops are not in a chain, and null must stay null rather than become "1 of 1".</summary>
    [Fact]
    public async Task A_stop_with_no_chain_stays_that_way()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);

        var outcome = await repository.SaveAsync(Topic("plain", null), Guid.CreateVersion7(), default);
        var editable = await repository.EditableAsync(outcome.Id, default);

        Assert.Null(editable!.Sequence);

        await transaction.RollbackAsync();
    }

    /// <summary>
    /// The badge must be removable, not just settable.
    /// </summary>
    /// <remarks>
    /// This is the one an `if (command.Sequence is not null)` in the repository would break — and it would
    /// break quietly: setting works, clearing appears to work, and the badge is still there after a reload.
    /// </remarks>
    [Fact]
    public async Task An_author_can_take_the_badge_back_off()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);

        var first = await repository.SaveAsync(
            Topic("undo", new SequenceCommand("OOP", 1, 3)), Guid.CreateVersion7(), default);

        var saved = await repository.EditableAsync(first.Id, default);
        Assert.NotNull(saved!.Sequence);

        var cleared = Topic("undo", null) with { Id = first.Id, RowVersion = saved.RowVersion };
        var second = await repository.SaveAsync(cleared, Guid.CreateVersion7(), default);

        Assert.Null(second.Error);

        var after = await repository.EditableAsync(first.Id, default);
        Assert.Null(after!.Sequence);

        await transaction.RollbackAsync();
    }

    /// <summary>
    /// "OOP IV / III" — the one that actually bites.
    /// </summary>
    /// <remarks>
    /// Every field is individually sane: a non-empty group, a positive part, a chain of three. Only the
    /// RELATIONSHIP between them is wrong, which is exactly the kind of thing no type catches and no author
    /// notices — they renumber a chain from four parts to three and forget the last stop.
    ///
    /// The reader is who pays: a badge reading "IV / III" sends them looking for a part nobody will write.
    /// </remarks>
    [Fact]
    public async Task A_part_past_the_end_of_its_own_chain_is_refused()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var result = await HandlerFor(context).HandleAsync(
            Topic("overflow", new SequenceCommand("OOP", 4, 3)), Guid.CreateVersion7(), default);

        Assert.False(result.IsSuccess);

        // The field matters as much as the refusal: the studio puts the message beside the input that is
        // wrong, and "this topic" is not a thing an author can click on.
        Assert.Contains("sequence.part", FieldsOf(result));
        Assert.Contains("4 of 3", MessagesOf(result));

        await transaction.RollbackAsync();
    }

    /// <summary>A chain of one is a badge promising a part that does not exist.</summary>
    [Fact]
    public async Task A_chain_of_one_is_refused()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var result = await HandlerFor(context).HandleAsync(
            Topic("solo", new SequenceCommand("OOP", 1, 1)), Guid.CreateVersion7(), default);

        Assert.False(result.IsSuccess);
        Assert.Contains("sequence.of", FieldsOf(result));

        await transaction.RollbackAsync();
    }

    /// <summary>Without a group there is nothing tying "OOP I" to "OOP II" — they are just numbered stops.</summary>
    [Fact]
    public async Task A_chain_with_no_group_is_refused()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var result = await HandlerFor(context).HandleAsync(
            Topic("nameless", new SequenceCommand("   ", 1, 3)), Guid.CreateVersion7(), default);

        Assert.False(result.IsSuccess);
        Assert.Contains("sequence.group", FieldsOf(result));

        await transaction.RollbackAsync();
    }

    /// <summary>
    /// A valid chain saves.
    /// </summary>
    /// <remarks>
    /// The one that stops the rules above from passing by refusing everything. Three refusal tests and no
    /// acceptance test would go green on <c>Require(false, …)</c>.
    /// </remarks>
    [Fact]
    public async Task A_well_formed_chain_saves()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var result = await HandlerFor(context).HandleAsync(
            Topic("fine", new SequenceCommand("OOP", 3, 3)), Guid.CreateVersion7(), default);

        Assert.True(result.IsSuccess);

        await transaction.RollbackAsync();
    }

    /// <summary>A stop with no chain must not be dragged through the chain rules at all.</summary>
    [Fact]
    public async Task No_chain_is_not_a_broken_chain()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var result = await HandlerFor(context).HandleAsync(
            Topic("nochain", null), Guid.CreateVersion7(), default);

        Assert.True(result.IsSuccess);

        await transaction.RollbackAsync();
    }

    private static IEnumerable<string> FieldsOf(WhyStack.Application.Common.Result<SaveTopicResult> result) =>
        result.Error!.FieldErrors!.Keys;

    private static string MessagesOf(WhyStack.Application.Common.Result<SaveTopicResult> result) =>
        string.Join(" ", result.Error!.FieldErrors!.Values.SelectMany(messages => messages));

    /// <summary>
    /// "OOP " and "OOP" must be one chain, not two.
    /// </summary>
    /// <remarks>
    /// The group is a KEY as much as a label — the parts are tied together by sharing it. A trailing space is
    /// invisible on screen and splits the chain in half everywhere it is counted.
    /// </remarks>
    [Fact]
    public async Task A_group_is_stored_trimmed()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);

        var outcome = await repository.SaveAsync(
            Topic("spacey", new SequenceCommand("  OOP  ", 1, 3)), Guid.CreateVersion7(), default);

        var editable = await repository.EditableAsync(outcome.Id, default);

        Assert.Equal("OOP", editable!.Sequence!.Group);

        await transaction.RollbackAsync();
    }
}
