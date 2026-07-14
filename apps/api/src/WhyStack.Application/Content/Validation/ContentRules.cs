using System.Text.RegularExpressions;

namespace WhyStack.Application.Content.Validation;

/// <summary>
/// The content rules, and there is exactly one implementation of them (ADR-0020, Decision 3).
/// </summary>
/// <remarks>
/// They used to live in TypeScript and run in CI. They live here now, because the gate moved: content is
/// authored in the application and saved through the API, so the API is the last place that can refuse it.
///
/// <b>The client does not enforce these. It ASKS.</b> `POST /content/topics/{id}/validate` runs exactly this
/// code and hands back the problems, so an editor sees them while typing instead of after a deploy. But the
/// save re-runs them, because a rule a client can skip is not a rule.
/// </remarks>
public static partial class ContentRules
{
    /// <summary>
    /// A table cell holds a FACT, not a paragraph (ADR-0019).
    /// </summary>
    /// <remarks>
    /// The reviewer's word for the first attempt was "göz yoran" — it hurts the eyes. He was right, and no
    /// renderer fixes it: a two-column table whose cells are full sentences is a comparison nobody can see,
    /// at any font size, on any device. Sixty characters is generous on purpose — the point is to catch a
    /// paragraph, not to police a phrase.
    /// </remarks>
    public const int MaxTableCellLength = 60;

    /// <summary>
    /// Every dictionary term used in the canonical text must survive, verbatim, into every translation.
    /// </summary>
    /// <remarks>
    /// "Does the Turkish text still say `Garbage Collector`?" is a FACT, not a judgement — and facts should
    /// be answered by something that is right every time. A model would answer correctly most of the time,
    /// and the times it did not would be silent, fluent, and indistinguishable from the times it did. That
    /// is the worst possible property for a gate.
    ///
    /// Turkish agglutinates, so the suffix belongs to the language and not to the term: `Middleware'in` and
    /// `Connection Pool'daki` are the term, intact, with a case ending. A whole-word matcher would call both
    /// of them missing and force authors to write mechanical Turkish to satisfy a validator — at which
    /// point they would turn the validator off, and rightly.
    /// </remarks>
    public static bool ContainsTerm(string text, string term)
    {
        var escaped = Regex.Escape(term);

        // Word boundary, the term, then optionally an apostrophe and a suffix. \p{L} rather than \w, because
        // \w does not cover ı, ğ, ş, ç, ö, ü.
        var pattern = $@"(?<![\p{{L}}\d]){escaped}(?:['’]\p{{L}}+)?(?![\p{{L}}\d])";

        return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    /// <summary>The cells of every table in a block of Markdown.</summary>
    /// <remarks>
    /// Fenced code blocks are skipped. A pipe inside one is a bitwise OR, a shell pipeline or an ASCII
    /// diagram — not a table, and measuring it as one would fail a build over a code sample.
    /// </remarks>
    public static IEnumerable<string> TableCells(string markdown)
    {
        var inFence = false;

        foreach (var raw in markdown.Split('\n'))
        {
            var line = raw.Trim();

            if (line.StartsWith("```", StringComparison.Ordinal))
            {
                inFence = !inFence;
                continue;
            }

            if (inFence) continue;
            if (!line.StartsWith('|') || !line.EndsWith('|')) continue;

            // `|---|:--:|` — the alignment row is not a cell.
            if (AlignmentRow().IsMatch(line)) continue;

            foreach (var cell in line[1..^1].Split('|'))
            {
                var text = cell.Trim();
                if (text.Length > 0) yield return text;
            }
        }
    }

    [GeneratedRegex(@"^\|[\s:|-]+\|$")]
    private static partial Regex AlignmentRow();
}
