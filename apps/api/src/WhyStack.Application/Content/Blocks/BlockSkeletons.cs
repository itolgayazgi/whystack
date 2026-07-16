using WhyStack.Application.Content.Validation;
using WhyStack.Domain.Content;

namespace WhyStack.Application.Content.Blocks;

/// <summary>
/// What an archetype's block skeleton starts with, and the four blocks every topic must have (ADR-0024).
/// </summary>
/// <remarks>
/// The skeleton is a SUGGESTION the editor fills and reshapes — it is not a fixed template, which is the whole
/// reason the section model was abandoned. The completeness rule, on the other hand, is a GATE: a topic may not
/// be published without it.
/// </remarks>
public static class BlockSkeletons
{
    /// <summary>
    /// Required before publish, regardless of archetype (ADR-0024).
    /// </summary>
    /// <remarks>
    /// A hook because a topic opens with a question, not a definition (ADR-0019). A checkpoint because passive
    /// reading is not learning. A summary because the reader must leave with something. A next because there
    /// are no dead ends — every station has a continuation.
    /// </remarks>
    public static readonly IReadOnlyList<BlockType> Mandatory =
        [BlockType.Hook, BlockType.Checkpoint, BlockType.Summary, BlockType.Next];

    /// <summary>The starting block order an archetype scaffolds. The editor reshapes it from here.</summary>
    public static IReadOnlyList<BlockType> For(Archetype archetype) => archetype switch
    {
        Archetype.Concept =>
        [
            BlockType.Hook, BlockType.Story, BlockType.Concept, BlockType.Code,
            BlockType.Myth, BlockType.Checkpoint, BlockType.Summary, BlockType.Next,
        ],

        Archetype.Mechanism =>
        [
            BlockType.Hook, BlockType.Concept, BlockType.Code, BlockType.Diagram,
            BlockType.Myth, BlockType.Checkpoint, BlockType.Prod, BlockType.Summary, BlockType.Next,
        ],

        Archetype.Comparison =>
        [
            BlockType.Hook, BlockType.Story, BlockType.Compare, BlockType.Code,
            BlockType.Checkpoint, BlockType.Summary, BlockType.Next,
        ],

        Archetype.Incident =>
        [
            BlockType.Hook, BlockType.Story, BlockType.Diagram, BlockType.Concept,
            BlockType.Prod, BlockType.Checkpoint, BlockType.Summary, BlockType.Next,
        ],

        Archetype.Pattern =>
        [
            BlockType.Hook, BlockType.Story, BlockType.Concept, BlockType.Diagram,
            BlockType.Code, BlockType.Myth, BlockType.Checkpoint, BlockType.Summary, BlockType.Next,
        ],

        Archetype.Workshop =>
        [
            BlockType.Hook, BlockType.Code, BlockType.Checkpoint, BlockType.Code,
            BlockType.Prod, BlockType.Summary, BlockType.Next,
        ],

        _ => [BlockType.Hook, BlockType.Checkpoint, BlockType.Summary, BlockType.Next],
    };

    /// <summary>
    /// Which mandatory blocks a draft is still missing, in the canonical language. The publish gate.
    /// </summary>
    /// <remarks>
    /// Checked against the SHARED blocks and the reader's chosen ecosystem alike — a hook is shared, so it
    /// counts wherever it sits; but the point is that the four beats exist somewhere in the flow.
    /// </remarks>
    public static IReadOnlyList<ContentProblem> MissingMandatory(IReadOnlyCollection<BlockType> present)
    {
        var have = present.ToHashSet();

        return
        [
            .. Mandatory
                .Where(required => !have.Contains(required))
                .Select(required => new ContentProblem(
                    $"blocks.{required}",
                    "block.mandatory-missing",
                    $"A topic needs a {required} block before it can be published (ADR-0024). "
                    + Reason(required))),
        ];
    }

    private static string Reason(BlockType type) => type switch
    {
        BlockType.Hook => "A topic opens with a question, not a definition.",
        BlockType.Checkpoint => "Passive reading is not learning.",
        BlockType.Summary => "The reader must leave with something.",
        BlockType.Next => "No dead ends — every station has a continuation.",
        _ => string.Empty,
    };
}
