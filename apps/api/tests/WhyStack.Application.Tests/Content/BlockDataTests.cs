using WhyStack.Application.Content.Blocks;
using WhyStack.Domain.Content;

namespace WhyStack.Application.Tests.Content;

/// <summary>
/// The block-data gate (ADR-0024). The database validates nothing about a block's JSON, so this does.
/// </summary>
public class BlockDataTests
{
    [Fact]
    public void A_well_formed_checkpoint_is_accepted()
    {
        var json = BlockData.Serialize(new CheckpointData(
            Question: "What does the thread do during an awaited I/O call?",
            Options: ["It blocks", "It returns to the pool", "It spawns a new thread"],
            CorrectIndex: 1,
            Explanation: "The thread is freed to serve other requests."));

        Assert.Empty(BlockData.Validate(BlockType.Checkpoint, json, order: 0));
    }

    /// <summary>
    /// A checkpoint whose correct answer points nowhere is the exact corruption a schemaless column invites.
    /// </summary>
    /// <remarks>
    /// <c>correctIndex = 5</c> against three options is a question that can never be answered right. If the
    /// database enforced the shape this test would be redundant — it does not, which is why the gate is here,
    /// and why removing it must break this.
    /// </remarks>
    [Fact]
    public void A_checkpoint_whose_correctIndex_is_out_of_range_is_refused()
    {
        var json = BlockData.Serialize(new CheckpointData(
            Question: "Pick one.",
            Options: ["a", "b", "c"],
            CorrectIndex: 5,
            Explanation: "…"));

        var problems = BlockData.Validate(BlockType.Checkpoint, json, order: 0);

        Assert.Contains(problems, problem => problem.Rule == "block.invalid-field");
    }

    [Fact]
    public void A_checkpoint_with_one_option_is_refused()
    {
        var json = BlockData.Serialize(new CheckpointData("Q", ["only one"], 0, "why"));

        var problems = BlockData.Validate(BlockType.Checkpoint, json, order: 0);

        Assert.Contains(problems, problem => problem.Rule == "block.missing-field");
    }

    [Fact]
    public void A_hook_must_be_a_question()
    {
        // Empty question — a hook that opens with nothing is not a hook (ADR-0019).
        var json = BlockData.Serialize(new HookData(Question: ""));

        Assert.Contains(
            BlockData.Validate(BlockType.Hook, json, order: 0),
            problem => problem.Rule == "block.missing-field");
    }

    [Fact]
    public void A_code_block_needs_a_language_and_source()
    {
        var json = BlockData.Serialize(new CodeData(Lang: "", Source: ""));

        Assert.Equal(2, BlockData.Validate(BlockType.Code, json, order: 0).Count);
    }

    [Fact]
    public void Malformed_json_is_a_problem_not_an_exception()
    {
        var problems = BlockData.Validate(BlockType.Concept, "{ not json", order: 3);

        Assert.Contains(problems, problem => problem.Rule == "block.invalid-json");
        Assert.All(problems, problem => Assert.Equal("blocks[3]", problem.Field));
    }

    [Fact]
    public void The_four_mandatory_blocks_are_required_to_publish()
    {
        // A draft with only a hook is missing checkpoint, summary and next.
        var missing = BlockSkeletons.MissingMandatory([BlockType.Hook, BlockType.Concept, BlockType.Code]);

        Assert.Equal(3, missing.Count);
        Assert.Contains(missing, problem => problem.Field == "blocks.Checkpoint");
        Assert.Contains(missing, problem => problem.Field == "blocks.Summary");
        Assert.Contains(missing, problem => problem.Field == "blocks.Next");
    }

    [Fact]
    public void A_flow_with_all_four_beats_passes_the_publish_gate()
    {
        var missing = BlockSkeletons.MissingMandatory(
            [BlockType.Hook, BlockType.Concept, BlockType.Checkpoint, BlockType.Summary, BlockType.Next]);

        Assert.Empty(missing);
    }

    [Fact]
    public void Every_archetype_scaffolds_the_four_mandatory_beats()
    {
        // A skeleton that did not already contain the mandatory blocks would hand the author a draft that
        // cannot be published without additions the tool never showed them.
        foreach (var archetype in Enum.GetValues<Archetype>())
        {
            var skeleton = BlockSkeletons.For(archetype).ToHashSet();

            Assert.True(
                BlockSkeletons.Mandatory.All(skeleton.Contains),
                $"{archetype}'s skeleton is missing a mandatory block.");
        }
    }
}
