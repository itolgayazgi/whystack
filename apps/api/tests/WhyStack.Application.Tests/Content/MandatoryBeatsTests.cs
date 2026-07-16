using WhyStack.Application.Content.Blocks;
using WhyStack.Domain.Content;

namespace WhyStack.Application.Tests.Content;

/// <summary>
/// The four beats a topic cannot go to review without (ADR-0024): Hook, Checkpoint, Summary, Next.
/// </summary>
/// <remarks>
/// This rule existed in an ADR and in a function, and in no code path a topic ever travelled through —
/// <c>MissingMandatory</c> was written, documented as "the publish gate", and called by nobody. Both of the
/// first two topics published with no Checkpoint at all.
///
/// It stopped being cosmetic the moment completion became a correct Checkpoint answer (the owner's call): a
/// topic with no Checkpoint is a topic nobody can ever finish. The basamak would never fill, no station
/// would ever go gold, and nothing anywhere would explain it.
/// </remarks>
public class MandatoryBeatsTests
{
    [Fact]
    public void A_topic_with_all_four_beats_has_nothing_to_answer_for()
    {
        var problems = BlockSkeletons.MissingMandatory(
            [BlockType.Hook, BlockType.Story, BlockType.Checkpoint, BlockType.Summary, BlockType.Next]);

        Assert.Empty(problems);
    }

    [Fact]
    public void A_topic_with_no_checkpoint_is_a_topic_nobody_can_finish()
    {
        var problems = BlockSkeletons.MissingMandatory([BlockType.Hook, BlockType.Summary, BlockType.Next]);

        var problem = Assert.Single(problems);

        // The field matters: the studio puts the message next to the thing that is wrong, and "blocks" as a
        // whole is not a thing an editor can click on.
        Assert.Equal("blocks.Checkpoint", problem.Field);
        Assert.Equal("block.mandatory-missing", problem.Rule);
    }

    [Theory]
    [InlineData(nameof(BlockType.Hook))]
    [InlineData(nameof(BlockType.Checkpoint))]
    [InlineData(nameof(BlockType.Summary))]
    [InlineData(nameof(BlockType.Next))]
    public void Every_one_of_the_four_is_actually_required(string missing)
    {
        var present = BlockSkeletons.Mandatory.Where(beat => beat.ToString() != missing).ToList();

        // Each beat gets its own case rather than one test that drops Checkpoint. A list of four where only
        // one is enforced passes a single-case test and is wrong three ways.
        var problem = Assert.Single(BlockSkeletons.MissingMandatory(present));

        Assert.Equal($"blocks.{missing}", problem.Field);
    }

    [Fact]
    public void An_empty_topic_is_told_about_all_four_at_once()
    {
        var problems = BlockSkeletons.MissingMandatory([]);

        // Not one at a time. An editor who fixes the hook and is then told about the checkpoint, and then
        // the summary, is being drip-fed a list we had in full from the start.
        Assert.Equal(4, problems.Count);
    }

    [Fact]
    public void The_reason_is_carried_with_the_rule()
    {
        var problem = Assert.Single(BlockSkeletons.MissingMandatory([BlockType.Hook, BlockType.Summary, BlockType.Next]));

        // "Checkpoint is required" tells an editor what to add. It does not tell them why, so the next
        // editor argues about it. Teaching Mode is not only for pull requests.
        Assert.Contains("Passive reading is not learning", problem.Message);
    }
}
