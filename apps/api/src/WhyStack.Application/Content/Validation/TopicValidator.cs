using WhyStack.Application.Content.Blocks;
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

    /// <summary>
    /// Checked before a topic may leave the draft. Everything above, plus completeness.
    /// </summary>
    /// <remarks>
    /// <b>Two models, and a topic uses one.</b> Blocks are the model (ADR-0024); sections are retired and have
    /// zero rows. Asking a block topic for sections is what made this gate impassable — a finished topic was
    /// refused for missing `LearningObjectives`, a field no editor has been offered a box for since the
    /// migration.
    ///
    /// So the completeness rules follow the model the topic actually uses. The RULES themselves are the same
    /// two in both worlds, and always were: what must be present, and whether a translation that exists is
    /// finished.
    /// </remarks>
    public IReadOnlyList<ContentProblem> ValidateForReview(TopicDraft draft, IReadOnlyCollection<SectionType> types)
    {
        var problems = new List<ContentProblem>(ValidateDraft(draft));

        if (draft.Blocks.Count > 0)
        {
            problems.AddRange(BlockCompleteness(draft));
            return problems;
        }

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

    /// <summary>
    /// The two completeness rules, for the model a topic actually uses (ADR-0024).
    /// </summary>
    /// <remarks>
    /// Both rules already existed for sections and neither had been carried across. That is the same failure
    /// twice: a rule written down, correct, and connected to nothing a topic passes through.
    ///
    /// <b>The four beats.</b> <see cref="BlockSkeletons.MissingMandatory"/> has always known this and was
    /// called only from /validate — the "Doğrula" button, which is advisory and skippable. That is how two
    /// topics shipped with no Checkpoint at all. It stopped being cosmetic when completion became a correct
    /// Checkpoint answer: a topic with no Checkpoint is a topic nobody can ever finish, so the basamak never
    /// fills, no station goes gold, and no screen explains why.
    ///
    /// <b>Translation completeness.</b> The reader's API reports a fallback when a language has NO blocks, not
    /// when it has fewer — <c>Blocks.Any(…)</c>. So a topic whose Turkish flow is one block short serves a
    /// Turkish reader a hole and says nothing. A half-translated topic is worse than an untranslated one: the
    /// reader concludes the app is broken rather than that the translation is unfinished.
    /// </remarks>
    private static IEnumerable<ContentProblem> BlockCompleteness(TopicDraft draft)
    {
        var problems = new List<ContentProblem>();

        var canonical = draft.Blocks
            .Where(block => block.LanguageCode == draft.CanonicalLanguage)
            .ToList();

        var present = canonical
            .Select(block => Enum.TryParse<BlockType>(block.Type, out var type) ? type : (BlockType?)null)
            .OfType<BlockType>()
            .ToList();

        problems.AddRange(BlockSkeletons.MissingMandatory(present));

        // Compared by POSITION, not by count. Six blocks in each is not the same claim as the same six
        // positions in each — an author who wrote a Turkish block at a position the English flow does not
        // have has two flows that merely happen to be the same length.
        var canonicalOrders = canonical.Select(block => block.Order).ToHashSet();

        foreach (var language in draft.Blocks.Select(block => block.LanguageCode).Distinct())
        {
            if (language == draft.CanonicalLanguage) continue;

            var translated = draft.Blocks
                .Where(block => block.LanguageCode == language)
                .Select(block => block.Order)
                .ToHashSet();

            foreach (var missing in canonicalOrders.Except(translated).OrderBy(order => order))
            {
                var type = canonical.First(block => block.Order == missing).Type;

                problems.Add(new ContentProblem(
                    $"blocks.{missing}.{language}",
                    "translation.incomplete",
                    $"Block {missing} ({type}) exists in {draft.CanonicalLanguage} and not in {language}. "
                    + "Finish it, or remove the block — a topic that changes language halfway through reads "
                    + "as a bug."));
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
/// <summary>
/// A topic as the publish gate sees it.
/// </summary>
/// <remarks>
/// <b>Blocks were missing from this record, and that is why nothing could be published.</b> ADR-0024 made
/// blocks the model and retired sections; `TopicSections` has had zero rows since. But this carried Sections
/// only, so the gate asked a finished block topic for twelve mandatory sections it does not have and cannot
/// have, refused it, and never once looked at the blocks — which are the content.
///
/// Both are here because one topic uses one model. Sections stay until the retired model is removed.
/// </remarks>
public sealed record TopicDraft(
    string CanonicalLanguage,
    IReadOnlyList<SectionDraft> Sections,
    IReadOnlyList<BlockDraft> Blocks);

public sealed record SectionDraft(string SectionTypeKey, string LanguageCode, string Markdown);

/// <summary>One block, as much of it as the gate needs: what beat it is, and which language it is in.</summary>
public sealed record BlockDraft(int Order, string Type, string LanguageCode);

/// <summary>An approved technical term. The term is preserved; only its explanation is translated.</summary>
public sealed record TerminologyEntry(
    string Term,
    IReadOnlyList<string> Aliases,
    IReadOnlyList<string> ForbiddenTranslations)
{
    /// <summary>Everything that counts as this term: the canonical spelling, plus its abbreviations.</summary>
    public IEnumerable<string> Spellings => new[] { Term }.Concat(Aliases);
}
