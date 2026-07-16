using WhyStack.Application.Common;
using WhyStack.Application.Content.Blocks;
using WhyStack.Application.Content.Validation;
using WhyStack.Domain.Content;

namespace WhyStack.Application.Content.Authoring;

/// <summary>
/// Validates a draft WITHOUT saving it — so the editor sees a problem while typing.
/// </summary>
/// <remarks>
/// <b>This is a courtesy, not the gate.</b> The gate is the save, and the transition. A rule a client can
/// skip is not a rule, and a client is a program anybody can replace with curl.
///
/// But the same code runs in all three places (ADR-0020, Decision 3). There is one implementation of "is
/// this content valid", and it lives here — so an editor cannot be told one thing while typing and a
/// different thing when they press Save. That divergence is the whole reason the rules moved out of a CI job
/// and into the layer that owns the gate.
///
/// <c>forReview</c> is the difference between "you are still writing" and "you are about to hand this to a
/// reviewer". A draft may be incomplete; a topic on its way out of the author's hands may not.
/// </remarks>
public sealed class ValidateTopicHandler(ITopicRepository topics)
{
    /// <summary>
    /// The language a topic is WRITTEN in before it is translated.
    /// </summary>
    /// <remarks>
    /// A constant rather than two string literals. The draft below and the beat check are both built on this
    /// assumption, and the day the canonical language becomes a per-topic choice, exactly one of the two
    /// would have been updated.
    /// </remarks>
    private const string CanonicalLanguage = "en";

    public async Task<Result<IReadOnlyList<ContentProblem>>> HandleAsync(
        SaveTopicCommand command,
        bool forReview,
        CancellationToken cancellationToken)
    {
        var terminology = await topics.TerminologyAsync(cancellationToken);
        var validator = new TopicValidator(terminology);

        var draft = new TopicDraft(
            CanonicalLanguage,
            [
                .. command.Sections.Select(section =>
                    new SectionDraft(section.SectionTypeKey, section.LanguageCode, section.Markdown)),

                // The implementation sections are validated too, and by the same rules. A translated term in
                // a `BasicExample` is exactly as wrong as one in `TradeOffs`, and a table of paragraphs is
                // exactly as unreadable.
                .. command.Implementations
                    .SelectMany(implementation => implementation.Sections)
                    .Select(section =>
                        new SectionDraft(section.SectionTypeKey, section.LanguageCode, section.Markdown)),
            ]);

        if (!forReview)
        {
            return Result<IReadOnlyList<ContentProblem>>.Success(validator.ValidateDraft(draft));
        }

        var catalog = await topics.CatalogAsync(cancellationToken);

        var sectionTypes = catalog.SectionTypes
            .Select(type => new SectionType
            {
                Key = type.Key,
                SortOrder = type.SortOrder,
                Scope = type.Scope,
                IsMandatory = type.IsMandatory,
                IsGraphDerived = type.IsGraphDerived,
            })
            .ToList();

        return Result<IReadOnlyList<ContentProblem>>.Success(
            [.. validator.ValidateForReview(draft, sectionTypes), .. MissingBeats(command)]);
    }

    /// <summary>
    /// The four beats ADR-0024 makes mandatory: Hook, Checkpoint, Summary, Next.
    /// </summary>
    /// <remarks>
    /// <b>This was written and never called.</b> BlockSkeletons.MissingMandatory's own summary calls itself
    /// "the publish gate", and for a while it was a gate with no wall beside it: a topic with no Checkpoint
    /// published happily, and both of the first two topics did exactly that. The rule existed in an ADR and
    /// in a function, and in no code path a topic ever travelled through.
    ///
    /// It matters more now than when it was written. Completion is a correct Checkpoint answer (the owner's
    /// call), so a topic with no Checkpoint is a topic nobody can ever finish — the basamak would simply
    /// never fill, and nothing anywhere would say why.
    ///
    /// Checked against the CANONICAL language only. A Turkish translation that has not been written yet is
    /// a translation problem; it is not a reason to say the topic has no hook.
    /// </remarks>
    private static IReadOnlyList<ContentProblem> MissingBeats(SaveTopicCommand command)
    {
        // A topic still on the old section model has no blocks to check. Reporting four missing beats on it
        // would be four problems the editor cannot act on, about a model they are not using.
        if (command.Blocks.Count == 0) return [];

        var present = command.Blocks
            .Where(block => block.LanguageCode == CanonicalLanguage)
            .Select(block => Enum.TryParse<BlockType>(block.Type, out var type) ? type : (BlockType?)null)
            .Where(type => type is not null)
            .Select(type => type!.Value)
            .ToList();

        return BlockSkeletons.MissingMandatory(present);
    }
}
