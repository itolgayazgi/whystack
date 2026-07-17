using Microsoft.EntityFrameworkCore;
using WhyStack.Application.Content.Authoring;
using WhyStack.Application.Content.Blocks;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Infrastructure.Tests;

/// <summary>
/// The gate a topic passes on its way to a reader (CLAUDE.md §1.5).
/// </summary>
/// <remarks>
/// <b>It used to check the retired model, and nothing could be published.</b> TransitionTopicHandler validates
/// a <c>TopicDraft</c>, and TopicDraft carried <c>Sections</c> and nothing else. ADR-0024 replaced sections
/// with blocks; TopicSections has zero rows. So the gate asked a finished block topic for twelve mandatory
/// sections it cannot have, refused it, and never looked at the blocks — which are the content. The owner hit
/// it on the first topic he wrote, with twelve errors naming fields no editor has been offered a box for.
///
/// Two rules already existed for sections and neither had been carried across. That is the same failure twice:
///
/// 1. <b>The four beats</b> — BlockSkeletons.MissingMandatory has always known them, and the gate never asked.
///    It was called only from /validate: the "Doğrula" button, advisory and skippable. MandatoryBeatsTests
///    records that this rule was once "documented as the publish gate and called by nobody", and that the
///    first two topics published with no Checkpoint at all.
/// 2. <b>Translation completeness</b> — "a translation that exists must be COMPLETE" was enforced for
///    sections and not for blocks, while the reader's API reports a fallback only when a language has NO
///    blocks. A Turkish flow one block short served a hole in silence.
///
/// Both are enforced here now, by the transition, on the model the topic actually uses.
/// </remarks>
[Collection(DatabaseCollection.Name)]
public sealed class PublishGateTests(DatabaseFixture fixture)
{
    private const string TestLine = "b1-language-runtime";

    private static BlockCommand Block(int order, string type, string dataJson) =>
        new(order, type, "en", null, dataJson);

    /// <summary>The four beats, in BOTH languages — a topic that is, by ADR-0024, finished.</summary>
    private static readonly BlockCommand[] Beats =
    [
        Block(1, "Hook", """{"question":"Why does this exist?"}"""),
        Block(2, "Checkpoint", """{"question":"Which is true?","options":["A","B"],"correctIndex":0,"explanation":"Because A."}"""),
        Block(3, "Summary", """{"items":["It exists."]}"""),
        Block(4, "Next", """{"label":"The next stop"}"""),
    ];

    private static BlockCommand Translated(BlockCommand block) =>
        block with { LanguageCode = "tr" };

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
        Blocks: [.. Beats, .. Beats.Select(Translated)],
        Sections: [],
        Implementations: [],
        Relationships: [],
        RowVersion: null);

    private static TransitionTopicHandler HandlerFor(WhyStackDbContext context) =>
        new(new ContentAuthoringRepository(context, TimeProvider.System), new TopicRepository(context));

    private static async Task<Guid> SeedAsync(WhyStackDbContext context, SaveTopicCommand command)
    {
        var outcome = await new ContentAuthoringRepository(context, TimeProvider.System)
            .SaveAsync(command, Guid.CreateVersion7(), default);

        Assert.Null(outcome.Error);

        return outcome.Id;
    }

    /// <summary>
    /// A REAL reviewer.
    /// </summary>
    /// <remarks>
    /// A made-up Guid fails on <c>FK_TopicReviews_Users_ReviewerId</c> — and it fails AFTER the gate has
    /// passed, which is how the first version of these tests looked like the gate still refusing. The row is
    /// the trace of a human decision, so the human has to exist; that the FK insists is the audit trail
    /// working, not an obstacle.
    /// </remarks>
    private static async Task<Guid> ReviewerAsync(WhyStackDbContext context) =>
        await context.Users.Select(user => user.Id).FirstAsync();

    /// <summary>
    /// A finished topic goes to review.
    /// </summary>
    /// <remarks>
    /// This assertion was <c>Assert.False</c> yesterday, and the file existed to say so. All four beats, both
    /// languages, nothing wrong with it — refused for having no <c>LearningObjectives</c>, a section from the
    /// model ADR-0024 retired.
    /// </remarks>
    [Fact]
    public async Task A_finished_block_topic_goes_to_review()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var id = await SeedAsync(context, Complete("complete"));

        var result = await HandlerFor(context).HandleAsync(
            id, "TechnicalReview", null, await ReviewerAsync(context), default);

        Assert.True(
            result.IsSuccess,
            "A topic with all four beats in both languages was refused: "
            + string.Join("; ", result.Error?.FieldErrors?.Keys ?? []));

        await transaction.RollbackAsync();
    }

    /// <summary>
    /// No Checkpoint, no review — enforced by the TRANSITION, not by a button.
    /// </summary>
    /// <remarks>
    /// The rule existed and the gate never asked it: MissingMandatory was called only from /validate, the
    /// "Doğrula" button, which an author can skip. That is how two topics shipped with no Checkpoint.
    ///
    /// It matters because completion is a correct Checkpoint answer (the owner's call). A topic without one is
    /// a topic nobody can finish: the basamak never fills, no station goes gold, and no screen explains why.
    /// </remarks>
    [Fact]
    public async Task A_topic_with_no_checkpoint_cannot_go_to_review()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var beats = Beats.Where(block => block.Type != "Checkpoint").ToArray();

        var id = await SeedAsync(
            context,
            Complete("no-checkpoint") with { Blocks = [.. beats, .. beats.Select(Translated)] });

        var result = await HandlerFor(context).HandleAsync(
            id, "TechnicalReview", null, await ReviewerAsync(context), default);

        Assert.False(result.IsSuccess);

        // The field names the BLOCK, so the studio can put the message beside the thing that is missing.
        Assert.Contains("blocks.Checkpoint", result.Error!.FieldErrors!.Keys);

        await transaction.RollbackAsync();
    }

    /// <summary>
    /// A half-translated topic cannot go to review.
    /// </summary>
    /// <remarks>
    /// The rule said so for sections and had never been carried to blocks. And the reader's API cannot save
    /// us: it reports a fallback when a language has NO blocks, not when it has fewer — <c>Blocks.Any(…)</c> —
    /// so a Turkish flow one block short serves a hole and says nothing.
    /// </remarks>
    [Fact]
    public async Task A_topic_whose_turkish_flow_is_short_cannot_go_to_review()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Every beat in English; the Summary never translated. Exactly what the old single-language add button
        // produced, and exactly what nothing anywhere reported.
        var turkish = Beats.Where(block => block.Type != "Summary").Select(Translated);

        var id = await SeedAsync(
            context,
            Complete("half-translated") with { Blocks = [.. Beats, .. turkish] });

        var result = await HandlerFor(context).HandleAsync(
            id, "TechnicalReview", null, await ReviewerAsync(context), default);

        Assert.False(result.IsSuccess);
        Assert.Contains("blocks.3.tr", result.Error!.FieldErrors!.Keys);

        await transaction.RollbackAsync();
    }

    /// <summary>
    /// An English-only topic is NOT half-translated — it is untranslated, and that is allowed.
    /// </summary>
    /// <remarks>
    /// The rule is "a translation that EXISTS must be complete", not "everything must be translated". Refusing
    /// this would make the rule mean something nobody decided, and would block every topic on its way to its
    /// first language.
    /// </remarks>
    [Fact]
    public async Task An_untranslated_topic_is_not_a_half_translated_one()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var id = await SeedAsync(context, Complete("english-only") with { Blocks = [.. Beats] });

        var result = await HandlerFor(context).HandleAsync(
            id, "TechnicalReview", null, await ReviewerAsync(context), default);

        Assert.True(
            result.IsSuccess,
            "An English-only topic was refused: "
            + string.Join("; ", result.Error?.FieldErrors?.Keys ?? []));

        await transaction.RollbackAsync();
    }

    /// <summary>The rule the gate now asks, at its source.</summary>
    [Fact]
    public void The_beats_rule_names_the_block_that_is_missing()
    {
        var problems = BlockSkeletons.MissingMandatory(
            [Domain.Content.BlockType.Hook, Domain.Content.BlockType.Summary, Domain.Content.BlockType.Next]);

        var problem = Assert.Single(problems);
        Assert.Equal("blocks.Checkpoint", problem.Field);
    }
}
