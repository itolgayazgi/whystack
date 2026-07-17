using WhyStack.Application.Content.Authoring;
using WhyStack.Application.Content.Blocks;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Infrastructure.Tests;

/// <summary>
/// The gate a topic passes on its way to a reader (CLAUDE.md §1.5).
/// </summary>
/// <remarks>
/// <b>It is checking the retired model.</b> TransitionTopicHandler validates a <c>TopicDraft</c>, and a
/// TopicDraft carries <c>Sections</c> and nothing else. ADR-0024 replaced sections with BLOCKS; the
/// TopicSections table has zero rows and every topic written since is a block flow. So the gate asks a
/// block-based topic for twelve mandatory sections it does not have and cannot have, and refuses it — while
/// never once looking at the blocks, which are the actual content.
///
/// Two failures in one, and they point opposite ways:
///
/// 1. <b>Nothing can be published.</b> The first step out of AiDraft fails with twelve errors naming sections
///    the model retired.
/// 2. <b>The rule that matters is not enforced.</b> The four mandatory beats (ADR-0024) — Hook, Checkpoint,
///    Summary, Next — are checked by BlockSkeletons.MissingMandatory, which the gate never calls. It is called
///    only from /validate, which is the "Doğrula" button: advisory, and skippable.
///
/// MandatoryBeatsTests records that this rule was once "documented as the publish gate and called by nobody",
/// and that the first two topics published with no Checkpoint at all. It is half-fixed: wired to the advisory
/// endpoint, still not to the enforcing one.
/// </remarks>
[Collection(DatabaseCollection.Name)]
public sealed class PublishGateTests(DatabaseFixture fixture)
{
    private const string TestLine = "b1-language-runtime";

    private static BlockCommand Block(int order, string type, string dataJson) =>
        new(order, type, "en", null, dataJson);

    /// <summary>A topic with all four mandatory beats — a topic that is, by ADR-0024, finished.</summary>
    private static SaveTopicCommand Complete(string suffix) => new(
        Id: null,
        StableKey: $"backend.gate-test-{suffix}",
        Slug: $"gate-test-{suffix}",
        LineKey: TestLine,
        ScopeKey: null,
        Category: "Concept",
        Archetype: "Concept",
        Sequence: null,
        Level: "Junior",
        EstimatedReadingMinutes: 6,
        SupportedVersions: [],
        Translations: [new TranslationCommand("en", "Gate fixture", "A fixture stop.")],
        Blocks:
        [
            Block(1, "Hook", """{"question":"Why does this exist?"}"""),
            Block(2, "Checkpoint", """{"question":"Which is true?","options":["A","B"],"correctIndex":0,"explanation":"Because A."}"""),
            Block(3, "Summary", """{"items":["It exists."]}"""),
            Block(4, "Next", """{"label":"The next stop"}"""),
        ],
        Sections: [],
        Implementations: [],
        Relationships: [],
        RowVersion: null);

    private static TransitionTopicHandler HandlerFor(WhyStackDbContext context) =>
        new(new ContentAuthoringRepository(context, TimeProvider.System), new TopicRepository(context));

    /// <summary>
    /// A finished topic cannot leave AiDraft.
    /// </summary>
    /// <remarks>
    /// It has all four beats. There is nothing wrong with it. It is refused because it has no
    /// <c>LearningObjectives</c> section — a section type from the model ADR-0024 retired, which no editor has
    /// been offered a box for since.
    /// </remarks>
    [Fact]
    public async Task A_finished_block_topic_cannot_be_sent_to_review()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);
        var outcome = await repository.SaveAsync(Complete("complete"), Guid.CreateVersion7(), default);

        Assert.Null(outcome.Error);

        var result = await HandlerFor(context).HandleAsync(
            outcome.Id, "TechnicalReview", null, Guid.CreateVersion7(), default);

        // THE FINDING. Delete this assertion when the gate reads blocks — it documents the bug, not the rule.
        Assert.False(result.IsSuccess);

        var fields = result.Error!.FieldErrors!.Keys.ToList();

        // Every complaint names a SECTION. Not one names a block, and the blocks are the topic.
        Assert.All(fields, field => Assert.StartsWith("sections.", field, StringComparison.Ordinal));

        await transaction.RollbackAsync();
    }

    /// <summary>
    /// The rule the gate should be enforcing is enforced somewhere else, and only on request.
    /// </summary>
    /// <remarks>
    /// <c>MissingMandatory</c> knows a topic with no Checkpoint is a topic nobody can ever finish — completion
    /// is a correct Checkpoint answer (the owner's call), so the basamak would never fill and no station would
    /// ever go gold, with nothing on any screen to explain it. The gate does not ask it.
    /// </remarks>
    [Fact]
    public void The_beats_rule_exists_and_the_gate_does_not_ask_it()
    {
        var problems = BlockSkeletons.MissingMandatory(
            [Domain.Content.BlockType.Hook, Domain.Content.BlockType.Summary, Domain.Content.BlockType.Next]);

        // The rule is real and it works — a missing Checkpoint is caught, right here, by a function the
        // transition never calls.
        var problem = Assert.Single(problems);
        Assert.Equal("blocks.Checkpoint", problem.Field);
    }
}
