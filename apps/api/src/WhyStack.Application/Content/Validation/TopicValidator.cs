using WhyStack.Domain.Content;

namespace WhyStack.Application.Content.Validation;

/// <summary>A problem an editor can act on: the field, the rule, and the sentence that says what to do.</summary>
public sealed record ContentProblem(string Field, string Rule, string Message);

/// <summary>
/// What must be true before a topic may advance out of a draft (ADR-0020).
/// </summary>
/// <remarks>
/// The rules are not applied at the same strength at every stage, and that is deliberate.
///
/// A draft is allowed to be incomplete — an author saves a half-written topic twenty times an hour, and a
/// validator that refused every one of those would be a validator they route around. What is refused is
/// PUBLISHING an incomplete topic: the gate is the transition, not the keystroke.
///
/// Two rules bite immediately, at every save, because they are cheap to fix now and expensive to fix later:
/// a translated technical term, and a table cell that is a paragraph.
/// </remarks>
public sealed class TopicValidator(IReadOnlyCollection<TerminologyEntry> dictionary)
{
    /// <summary>Checked on every save. Cheap now, expensive after a hundred topics.</summary>
    public IReadOnlyList<ContentProblem> ValidateDraft(TopicDraft draft)
    {
        var problems = new List<ContentProblem>();

        foreach (var section in draft.Sections)
        {
            problems.AddRange(TableCellRule(section));
        }

        problems.AddRange(TerminologyRule(draft));

        return problems;
    }

    /// <summary>Checked before a topic may leave AiDraft. Everything above, plus completeness.</summary>
    public IReadOnlyList<ContentProblem> ValidateForReview(TopicDraft draft, IReadOnlyCollection<SectionType> types)
    {
        var problems = new List<ContentProblem>(ValidateDraft(draft));

        var written = draft.Sections
            .Where(section => section.LanguageCode == draft.CanonicalLanguage)
            .Select(section => section.SectionTypeKey)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var mandatory in types.Where(type => type.IsMandatory && !type.IsGraphDerived))
        {
            if (!written.Contains(mandatory.Key))
            {
                problems.Add(new ContentProblem(
                    $"sections.{mandatory.Key}",
                    "section.missing",
                    $"\"{mandatory.Key}\" is mandatory (10 § Mandatory Topic Sections). A topic that skips it is "
                    + "not a topic — it is a reference entry, and the internet already has those."));
            }
        }

        // A translation that exists must be COMPLETE. A half-translated topic is worse than an untranslated
        // one: the reader gets four sections in Turkish, then hits English with no warning, and concludes
        // the app is broken rather than that the translation is unfinished.
        foreach (var language in draft.Sections.Select(section => section.LanguageCode).Distinct())
        {
            if (language == draft.CanonicalLanguage) continue;

            var translated = draft.Sections
                .Where(section => section.LanguageCode == language)
                .Select(section => section.SectionTypeKey)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var missing in written.Except(translated))
            {
                problems.Add(new ContentProblem(
                    $"sections.{missing}.{language}",
                    "translation.incomplete",
                    $"\"{missing}\" exists in {draft.CanonicalLanguage} and not in {language}. Finish it, or "
                    + "remove the translation — a topic that changes language halfway through reads as a bug."));
            }
        }

        return problems;
    }

    private static IEnumerable<ContentProblem> TableCellRule(SectionDraft section)
    {
        foreach (var cell in ContentRules.TableCells(section.Markdown))
        {
            if (cell.Length <= ContentRules.MaxTableCellLength) continue;

            yield return new ContentProblem(
                $"sections.{section.SectionTypeKey}.{section.LanguageCode}",
                "prose.table-cell-too-long",
                $"A table cell is {cell.Length} characters: \"{cell[..40]}…\". A table cell holds a fact, not a "
                + $"paragraph (ADR-0019). Keep it under {ContentRules.MaxTableCellLength} characters, or write it "
                + "as prose — a comparison a reader cannot see side by side is not a comparison.");
        }
    }

    private IEnumerable<ContentProblem> TerminologyRule(TopicDraft draft)
    {
        var canonical = string.Join('\n', draft.Sections
            .Where(section => section.LanguageCode == draft.CanonicalLanguage)
            .Select(section => section.Markdown));

        if (canonical.Length == 0) yield break;

        // Only the terms this topic actually uses. Checking the whole dictionary against every topic would
        // demand that a topic about SQL mention `Garbage Collector` in Turkish because the dictionary knows
        // the word.
        var inUse = dictionary
            .Where(entry => entry.Spellings.Any(spelling => ContentRules.ContainsTerm(canonical, spelling)))
            .ToList();

        foreach (var language in draft.Sections.Select(section => section.LanguageCode).Distinct())
        {
            if (language == draft.CanonicalLanguage) continue;

            var text = string.Join('\n', draft.Sections
                .Where(section => section.LanguageCode == language)
                .Select(section => section.Markdown));

            foreach (var entry in inUse)
            {
                if (!entry.Spellings.Any(spelling => ContentRules.ContainsTerm(text, spelling)))
                {
                    yield return new ContentProblem(
                        $"terminology.{entry.Term}.{language}",
                        "terminology.translated",
                        $"The canonical text uses \"{entry.Term}\" and the {language} text does not. Technical "
                        + "terms are preserved; only their explanation is translated (10, Forbidden Pattern 06).");
                }

                // Present AND paraphrased is still a violation. A translator rarely drops a term outright —
                // it keeps it in the heading, where it looks diligent, and paraphrases it for five paragraphs.
                // The survival check above would pass that.
                foreach (var forbidden in entry.ForbiddenTranslations)
                {
                    if (ContentRules.ContainsTerm(text, forbidden))
                    {
                        yield return new ContentProblem(
                            $"terminology.{entry.Term}.{language}",
                            "terminology.forbidden",
                            $"\"{forbidden}\" is a translation of \"{entry.Term}\". Use \"{entry.Term}\" and "
                            + $"explain it in {language}.");
                    }
                }
            }
        }
    }
}

/// <summary>A topic as an editor has it on screen — before it is a row.</summary>
public sealed record TopicDraft(string CanonicalLanguage, IReadOnlyList<SectionDraft> Sections);

public sealed record SectionDraft(string SectionTypeKey, string LanguageCode, string Markdown);

/// <summary>An approved technical term. The term is preserved; only its explanation is translated.</summary>
public sealed record TerminologyEntry(
    string Term,
    IReadOnlyList<string> Aliases,
    IReadOnlyList<string> ForbiddenTranslations)
{
    /// <summary>Everything that counts as this term: the canonical spelling, plus its abbreviations.</summary>
    public IEnumerable<string> Spellings => new[] { Term }.Concat(Aliases);
}
