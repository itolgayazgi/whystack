using WhyStack.Application.Common;
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
    public async Task<Result<IReadOnlyList<ContentProblem>>> HandleAsync(
        SaveTopicCommand command,
        bool forReview,
        CancellationToken cancellationToken)
    {
        var terminology = await topics.TerminologyAsync(cancellationToken);
        var validator = new TopicValidator(terminology);

        var draft = new TopicDraft(
            "en",
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
            validator.ValidateForReview(draft, sectionTypes));
    }
}
