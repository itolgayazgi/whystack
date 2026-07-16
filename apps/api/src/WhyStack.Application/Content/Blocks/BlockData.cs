using System.Text.Json;
using WhyStack.Application.Content.Validation;
using WhyStack.Domain.Content;

namespace WhyStack.Application.Content.Blocks;

/// <summary>
/// The shape of every block type's <c>DataJson</c> (ADR-0024), and the gate that enforces it.
/// </summary>
/// <remarks>
/// The block body is a schemaless <c>nvarchar(max)</c> column — the flexibility the fixed section columns
/// could not give. The price (ADR-0024, Decision 6) is that the database validates nothing, so the shape is
/// enforced HERE, on every save. A schemaless column with no application gate is a way to store a checkpoint
/// with no correct answer, a code block with no source, a hook that is not a question.
///
/// The records below are the contract. Both platforms render from the same JSON (ADR-0022); the field names
/// here are the field names the web and native renderers read.
/// </remarks>
public static class BlockData
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Validate one block's data against its type. Returns the problems; empty means valid.
    /// </summary>
    /// <remarks>
    /// Parses defensively: a block whose JSON does not even parse is a problem, not an exception thrown three
    /// layers up. Every required field is named in the message, because the author is the one who fixes it.
    /// </remarks>
    public static IReadOnlyList<ContentProblem> Validate(BlockType type, string dataJson, int order)
    {
        var field = $"blocks[{order}]";

        JsonElement root;

        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(dataJson) ? "{}" : dataJson);
            root = document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return [new ContentProblem(field, "block.invalid-json", $"The {type} block's data is not valid JSON.")];
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            return [new ContentProblem(field, "block.invalid-json", $"The {type} block's data must be an object.")];
        }

        var problems = new List<ContentProblem>();

        void RequireText(string name)
        {
            if (!root.TryGetProperty(name, out var value)
                || value.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(value.GetString()))
            {
                problems.Add(new ContentProblem(
                    field, "block.missing-field", $"A {type} block needs a non-empty \"{name}\"."));
            }
        }

        int ArrayLength(string name) =>
            root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Array
                ? value.GetArrayLength()
                : -1;

        switch (type)
        {
            case BlockType.Hook:
                // A hook is a QUESTION — "why before how" (ADR-0019). Not a definition, not a paragraph.
                RequireText("question");
                break;

            case BlockType.Story or BlockType.Concept or BlockType.Prod:
                RequireText("markdown");
                break;

            case BlockType.Code:
                RequireText("lang");
                RequireText("source");
                break;

            case BlockType.Diagram:
                RequireText("svg");
                break;

            case BlockType.Compare:
                if (ArrayLength("headers") < 2)
                {
                    problems.Add(new ContentProblem(
                        field, "block.missing-field", "A compare block needs at least two column \"headers\"."));
                }

                if (ArrayLength("rows") < 1)
                {
                    problems.Add(new ContentProblem(
                        field, "block.missing-field", "A compare block needs at least one \"rows\" entry."));
                }

                break;

            case BlockType.Myth:
                RequireText("claim");
                RequireText("truth");
                break;

            case BlockType.Checkpoint:
                // The one that carries the most weight, and the one a schemaless column would most happily
                // corrupt: a checkpoint with no correct answer is a question that can never be right.
                RequireText("question");
                RequireText("explanation");

                var optionCount = ArrayLength("options");

                if (optionCount < 2)
                {
                    problems.Add(new ContentProblem(
                        field, "block.missing-field", "A checkpoint needs at least two \"options\"."));
                }
                else if (!root.TryGetProperty("correctIndex", out var correct)
                    || correct.ValueKind != JsonValueKind.Number
                    || correct.GetInt32() < 0
                    || correct.GetInt32() >= optionCount)
                {
                    problems.Add(new ContentProblem(
                        field,
                        "block.invalid-field",
                        $"\"correctIndex\" must point at one of the {optionCount} options (0–{optionCount - 1})."));
                }

                break;

            case BlockType.Term:
                RequireText("term");
                RequireText("definition");
                break;

            case BlockType.Summary:
                if (ArrayLength("items") < 1)
                {
                    problems.Add(new ContentProblem(
                        field, "block.missing-field", "A summary needs at least one \"items\" entry."));
                }

                break;

            case BlockType.Next:
                // No dead ends (ADR-0024): a next block must at least say where to go, even if the target is
                // not linked yet.
                RequireText("label");
                break;

            default:
                problems.Add(new ContentProblem(field, "block.unknown-type", $"Unknown block type \"{type}\"."));
                break;
        }

        return problems;
    }

    /// <summary>Serialize a block payload to the canonical JSON the column stores.</summary>
    public static string Serialize<T>(T payload) => JsonSerializer.Serialize(payload, Json);
}

/// <summary>The hook: a question that opens the topic. "Why before how" lives here (ADR-0019).</summary>
public sealed record HookData(string Question, string? Promise = null);

public sealed record ProseData(string Markdown, string? Analogy = null);

public sealed record CodeData(
    string Lang,
    string Source,
    string? File = null,
    IReadOnlyList<int>? HighlightLines = null,
    string? Annotation = null);

public sealed record DiagramData(string Svg, string? Caption = null);

public sealed record CompareData(
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<string>> Rows,
    string? Conclusion = null);

public sealed record MythData(string Claim, string Truth);

public sealed record CheckpointData(
    string Question,
    IReadOnlyList<string> Options,
    int CorrectIndex,
    string Explanation);

public sealed record TermData(string Term, string Definition, string? TermKey = null);

public sealed record SummaryData(IReadOnlyList<string> Items);

public sealed record NextData(
    string Label,
    string? ToStableKey = null,
    string? TransferStableKey = null,
    string? TransferReason = null);
