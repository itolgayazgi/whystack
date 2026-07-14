using WhyStack.Application.Common;
using WhyStack.Application.Content.Validation;
using WhyStack.Domain.Content;

namespace WhyStack.Application.Content.Authoring;

/// <summary>
/// Moves a topic through the editorial lifecycle. <b>This is the gate CLAUDE.md §1.5 talks about.</b>
/// </summary>
/// <remarks>
/// A model can write a topic. A model cannot publish one. This handler is where that stops being a
/// sentence in a document and starts being a 422.
///
/// Two things are refused here, and neither is negotiable:
///
/// 1. <b>A transition that skips a review.</b> `AiDraft → Published` is forbidden by name. The lifecycle is
///    ordered, and <see cref="ContentLifecycle.MayTransition"/> is arithmetic, not a promise.
///
/// 2. <b>Advancing a topic that is not finished.</b> Mandatory sections missing, a half-finished
///    translation, a term the translation dropped — a draft may have all of these, and a topic on its way to
///    a reader may not.
/// </remarks>
public sealed class TransitionTopicHandler(
    IContentAuthoringRepository repository,
    ITopicRepository topics)
{
    public async Task<Result<TransitionOutcome>> HandleAsync(
        Guid topicId,
        string toStatus,
        string? note,
        Guid reviewerId,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ContentStatus>(toStatus, out var target))
        {
            return Error.Validation("status", $"\"{toStatus}\" is not a status in 10's Topic Lifecycle.");
        }

        // Advancing THROUGH review, not merely to it: the completeness rules apply from the moment a topic
        // leaves the author's hands. A reviewer should be reading a finished topic, not finding the holes.
        if (target > ContentStatus.AiDraft)
        {
            var draft = await repository.DraftAsync(topicId, cancellationToken);

            if (draft is null)
            {
                return new Error(ErrorCodes.ResourceNotFound, "No such topic.");
            }

            var terminology = await topics.TerminologyAsync(cancellationToken);
            var sectionTypes = await topics.CatalogAsync(cancellationToken);

            var problems = new TopicValidator(terminology).ValidateForReview(
                draft,
                [
                    .. sectionTypes.SectionTypes.Select(type => new SectionType
                    {
                        Key = type.Key,
                        SortOrder = type.SortOrder,
                        Scope = type.Scope,
                        IsMandatory = type.IsMandatory,
                        IsGraphDerived = type.IsGraphDerived,
                    })
                ]);

            if (problems.Count > 0)
            {
                return Error.Validation(problems
                    .GroupBy(problem => problem.Field)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(problem => problem.Message).ToArray()));
            }
        }

        return await repository.TransitionAsync(topicId, toStatus, note, reviewerId, cancellationToken);
    }
}
