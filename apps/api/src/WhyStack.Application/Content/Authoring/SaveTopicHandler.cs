using WhyStack.Application.Common;
using WhyStack.Application.Content.Validation;

namespace WhyStack.Application.Content.Authoring;

/// <summary>Reads and writes content for the studio. EF Core stays on the far side of this line.</summary>
public interface IContentAuthoringRepository
{
    Task<SaveOutcome> SaveAsync(SaveTopicCommand command, Guid editorId, CancellationToken cancellationToken);

    Task<Result<TransitionOutcome>> TransitionAsync(
        Guid topicId,
        string toStatus,
        string? note,
        Guid reviewerId,
        CancellationToken cancellationToken);

    /// <summary>The sections, flattened — what the validator needs and nothing more.</summary>
    Task<TopicDraft?> DraftAsync(Guid topicId, CancellationToken cancellationToken);

    /// <summary>Every topic, at every stage. The editor's workbench, not a reader's list.</summary>
    Task<IReadOnlyList<StudioTopic>> StudioListAsync(CancellationToken cancellationToken);

    /// <summary>One topic, in full, for editing: every language, every implementation, the graph as keys.</summary>
    Task<EditableTopic?> EditableAsync(Guid topicId, CancellationToken cancellationToken);

    Task<IReadOnlyList<EditableTerm>> TermsAsync(CancellationToken cancellationToken);

    Task<Guid> SaveTermAsync(SaveTermCommand command, CancellationToken cancellationToken);

    /// <summary>False when the term does not exist. Deleting one that is not there is not an error.</summary>
    Task<bool> DeleteTermAsync(Guid termId, CancellationToken cancellationToken);

    /// <summary>The themes, each with a count of how many topics use it (ADR-0023).</summary>
    Task<IReadOnlyList<EditableScope>> SubAreasAsync(CancellationToken cancellationToken);

    Task<Guid> SaveScopeAsync(SaveScopeCommand command, CancellationToken cancellationToken);

    /// <summary>Refuses to delete a theme that still tags topics — it would silently untag them (ADR-0023).</summary>
    Task<DeleteSubAreaOutcome> DeleteScopeAsync(Guid subAreaId, CancellationToken cancellationToken);
}

public sealed record SaveOutcome(
    Guid Id,
    string Status,
    string RowVersion,
    bool Conflict,
    string? Error,

    /// <summary>Which field the error is about, so it lands on the right input rather than always on stableKey.</summary>
    string? ErrorField = null);

public sealed record TransitionOutcome(string Status);

/// <summary>
/// Save a topic. Validation runs HERE, on the server, on every save (ADR-0020, Decision 3).
/// </summary>
/// <remarks>
/// The client also validates — it calls the same rules through an endpoint, so the editor sees a problem
/// while typing instead of after a deploy. That is a courtesy. <b>This is the gate.</b> A rule a client can
/// skip is not a rule, and the client is a program anybody can replace with curl.
///
/// A DRAFT is allowed to be incomplete: an author saves a half-written topic twenty times an hour, and a
/// validator that refused every one of those is a validator they route around. What is refused is
/// PUBLISHING an incomplete topic — see <c>TransitionTopicHandler</c>. Two rules bite immediately anyway,
/// because they are cheap to fix now and expensive to fix at a hundred topics: a translated technical term,
/// and a table cell that is a paragraph.
/// </remarks>
public sealed class SaveTopicHandler(
    IContentAuthoringRepository repository,
    ITopicRepository topics)
{
    public async Task<Result<SaveTopicResult>> HandleAsync(
        SaveTopicCommand command,
        Guid editorId,
        CancellationToken cancellationToken)
    {
        var problems = Validate(command);

        if (problems.Any(problem => problem.Rule.StartsWith("metadata.", StringComparison.Ordinal)))
        {
            // Metadata problems are REFUSED, not reported. A topic with no stable key is not a draft with a
            // warning on it — it is a row the graph cannot address, and letting it exist means every edge that
            // ever points at it is broken from the moment it is written.
            return Error.Validation(problems.ToDictionary(
                problem => problem.Field,
                problem => new[] { problem.Message }));
        }

        // The block bodies. The column is schemaless (ADR-0024, Decision 6), so this is the only thing between
        // an editor and a checkpoint with no correct answer. A DRAFT is allowed to have these problems — they
        // come back WITH the saved topic — but they are computed on every save, not at publish, because a
        // hundred topics later is the wrong time to find out.
        var blockProblems = command.Blocks
            .SelectMany(block => Blocks.BlockData.Validate(
                Enum.TryParse<Domain.Content.BlockType>(block.Type, out var type) ? type : default,
                block.DataJson,
                block.Order))
            .ToList();

        var terminology = await topics.TerminologyAsync(cancellationToken);
        var validator = new TopicValidator(terminology);

        var content = validator.ValidateDraft(new TopicDraft(
            "en",
            [
                .. command.Sections.Select(section =>
                    new SectionDraft(section.SectionTypeKey, section.LanguageCode, section.Markdown)),
                .. command.Implementations.SelectMany(implementation => implementation.Sections)
                    .Select(section => new SectionDraft(section.SectionTypeKey, section.LanguageCode, section.Markdown)),
            ]));

        var outcome = await repository.SaveAsync(command, editorId, cancellationToken);

        if (outcome.Conflict)
        {
            // Somebody else saved between this editor's read and their write. Not an error they caused, and
            // not something to resolve for them — they are told, and they decide.
            return new Error(
                ErrorCodes.ConcurrencyConflict,
                "This topic was changed somewhere else while you were editing it. Reload to see what is "
                + "actually saved, then re-apply your change.");
        }

        if (outcome.Error is not null)
        {
            return Error.Validation(outcome.ErrorField ?? "stableKey", outcome.Error);
        }

        // The content problems come back WITH the saved topic, not instead of it. A draft is allowed to have
        // them; the editor is the one who fixes them, and they cannot fix what they cannot save.
        return Result<SaveTopicResult>.Success(
            new SaveTopicResult(outcome.Id, outcome.Status, outcome.RowVersion, [.. content, .. blockProblems]));
    }

    /// <summary>The shape, not the prose. A topic with no identity is not a draft — it is a broken row.</summary>
    private static List<ContentProblem> Validate(SaveTopicCommand command)
    {
        var problems = new List<ContentProblem>();

        void Require(bool ok, string field, string message)
        {
            if (!ok) problems.Add(new ContentProblem(field, "metadata.invalid", message));
        }

        Require(
            !string.IsNullOrWhiteSpace(command.StableKey),
            "stableKey",
            "A stable key is required. It is the topic's identity — every graph edge and every roadmap entry "
            + "resolves through it, and it never changes after this.");

        Require(
            IsSlug(command.Slug),
            "slug",
            "A slug is lowercase letters, digits and hyphens. It is the URL.");

        Require(
            !string.IsNullOrWhiteSpace(command.LineKey),
            "lineKey",
            "A topic belongs to a domain — Backend, Database — not to a language (ADR-0021).");

        Require(
            command.EstimatedReadingMinutes > 0,
            "estimatedReadingMinutes",
            "Reading time must be a positive number of minutes.");

        Require(
            command.Translations.Any(translation => translation.LanguageCode == "en"),
            "translations",
            "English is required. It is the canonical language, and a translation with no source cannot be "
            + "reviewed against anything.");

        // A chain, if the author declared one. Null is the normal case and says nothing is wrong.
        //
        // REFUSED rather than reported, like the rest of this method — and that is a choice worth its own
        // paragraph. A missing Checkpoint is INCOMPLETE: the author has not written it yet, and the to-do list
        // is the right answer. "OOP IV / III" is not incomplete, it is SELF-CONTRADICTORY — there is no part 4
        // of a 3-part chain, and no amount of further writing makes one.
        //
        // The deciding fact is that the publish gate (TransitionTopicHandler) validates the draft's SECTIONS
        // and nothing else. A sequence problem reported here would travel back to the studio, sit in a list,
        // and publish anyway — which is exactly how the mandatory beats came to be documented as "the publish
        // gate" while being called by nobody, and how the first two topics shipped with no Checkpoint at all.
        // A rule that does not stop anything is a comment.
        if (command.Sequence is { } sequence)
        {
            Require(
                !string.IsNullOrWhiteSpace(sequence.Group),
                "sequence.group",
                "A numbered chain needs a group — the name its parts share, like \"OOP\". It is what ties "
                + "\"OOP I\" to \"OOP II\"; without it each part is a stop that merely happens to be numbered.");

            Require(
                sequence.Part >= 1,
                "sequence.part",
                "A part is 1 or more. There is no part 0 of anything — the reader counts from one.");

            Require(
                sequence.Of >= 2,
                "sequence.of",
                "A chain has at least 2 parts. \"I / I\" is not a chain, it is one stop wearing a badge that "
                + "promises a second one that does not exist.");

            // The one that actually bites. Every field above can be individually sane and still produce
            // "OOP IV / III" — a badge that tells the reader to go looking for a part nobody will ever write.
            Require(
                sequence.Part <= sequence.Of,
                "sequence.part",
                $"Part {sequence.Part} of {sequence.Of} cannot exist. The part cannot be past the end of its "
                + "own chain.");
        }

        return problems;
    }

    private static bool IsSlug(string slug) =>
        !string.IsNullOrWhiteSpace(slug)
        && slug.All(character => char.IsAsciiLetterLower(character) || char.IsAsciiDigit(character) || character == '-');
}
