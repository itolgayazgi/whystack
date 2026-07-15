using Microsoft.EntityFrameworkCore;
using WhyStack.Application.Common;
using WhyStack.Application.Content.Authoring;
using WhyStack.Application.Content.Validation;
using WhyStack.Domain.Content;
using WhyStack.Domain.Users;

namespace WhyStack.Infrastructure.Persistence;

public sealed class ContentAuthoringRepository(WhyStackDbContext context, TimeProvider clock)
    : IContentAuthoringRepository
{
    public async Task<SaveOutcome> SaveAsync(
        SaveTopicCommand command,
        Guid editorId,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var domain = await context.KnowledgeDomains
            .SingleOrDefaultAsync(candidate => candidate.Key == command.DomainKey, cancellationToken);

        if (domain is null)
        {
            return new SaveOutcome(
                Guid.Empty, string.Empty, string.Empty, false, $"No domain \"{command.DomainKey}\".", "domainKey");
        }

        // A theme is OPTIONAL (null is fine), but a non-null one must exist. This is the gate: the studio
        // populates the dropdown from the catalog, so a bad key only arrives from a raw API call — and the
        // foreign key would reject it anyway. Resolving it here turns that into a clear 422 instead of a 500.
        Guid? subAreaId = null;

        if (!string.IsNullOrWhiteSpace(command.SubAreaKey))
        {
            var subArea = await context.SubAreas
                .SingleOrDefaultAsync(candidate => candidate.Key == command.SubAreaKey, cancellationToken);

            if (subArea is null)
            {
                return new SaveOutcome(
                    Guid.Empty, string.Empty, string.Empty, false,
                    $"No theme \"{command.SubAreaKey}\". Themes are managed in the studio.", "subAreaKey");
            }

            subAreaId = subArea.Id;
        }

        // Category and level are CLOSED enums, and the studio offers them as dropdowns — so from the UI these
        // always parse. But the client is a program anybody can replace with curl, and an unparseable value
        // reaching Enum.Parse is a 500 that blames the server for the caller's typo. Parse once, here, and
        // turn a bad value into the 422 it is.
        if (!Enum.TryParse<TopicCategory>(command.Category, out var category))
        {
            return new SaveOutcome(
                Guid.Empty, string.Empty, string.Empty, false,
                $"No category \"{command.Category}\".", "category");
        }

        if (!Enum.TryParse<SkillLevel>(command.Level, out var level))
        {
            return new SaveOutcome(
                Guid.Empty, string.Empty, string.Empty, false,
                $"No level \"{command.Level}\".", "level");
        }

        var topic = command.Id is null
            ? null
            : await context.Topics
                .Include(candidate => candidate.Versions).ThenInclude(version => version.Sections)
                .Include(candidate => candidate.Versions).ThenInclude(version => version.Translations)
                .Include(candidate => candidate.Versions).ThenInclude(version => version.SupportedVersions)
                .Include(candidate => candidate.Versions)
                    .ThenInclude(version => version.Implementations)
                    .ThenInclude(implementation => implementation.Sections)
                .SingleOrDefaultAsync(candidate => candidate.Id == command.Id, cancellationToken);

        if (command.Id is not null && topic is null)
        {
            return new SaveOutcome(Guid.Empty, string.Empty, string.Empty, false, "No such topic.");
        }

        TopicVersion version;

        if (topic is null)
        {
            topic = new Topic
            {
                Id = Guid.CreateVersion7(),
                StableKey = command.StableKey,
                Slug = command.Slug,
                DomainId = domain.Id,
                SubAreaId = subAreaId,
                Category = category,
                DefaultLevel = level,
                DefaultTitle = command.Translations.First(translation => translation.LanguageCode == "en").Title,
                CreatedAtUtc = now,
            };

            version = new TopicVersion
            {
                Id = Guid.CreateVersion7(),
                TopicId = topic.Id,
                VersionNumber = 1,

                // AiDraft, always, and never anything further. A topic is born unreviewed no matter who typed
                // it — CLAUDE.md §1.5 is about what has been READ, not about who did the typing, and an
                // editor creating a topic has not yet reviewed it either.
                Status = ContentStatus.AiDraft,

                CanonicalLanguageCode = "en",
                EstimatedReadingMinutes = command.EstimatedReadingMinutes,
                LastReviewedOn = DateOnly.FromDateTime(now),
                CreatedAtUtc = now,
            };

            topic.Versions.Add(version);
            context.Topics.Add(topic);
        }
        else
        {
            version = topic.Versions.OrderByDescending(candidate => candidate.VersionNumber).First();

            // Optimistic concurrency, and the one line that is it.
            //
            // EF builds the UPDATE's WHERE from the ORIGINAL value of the concurrency token. Overwriting that
            // with what the CLIENT last saw is what turns "save this topic" into "save this topic, given that
            // this is what I was looking at" — and it is why a second tab cannot silently revert this edit.
            if (!string.IsNullOrEmpty(command.RowVersion))
            {
                context.Entry(version).Property(entity => entity.RowVersion).OriginalValue =
                    Convert.FromBase64String(command.RowVersion);
            }

            topic.Slug = command.Slug;
            topic.DomainId = domain.Id;
            topic.SubAreaId = subAreaId;
            topic.Category = category;
            topic.DefaultLevel = level;
            topic.DefaultTitle = command.Translations.First(translation => translation.LanguageCode == "en").Title;
            topic.UpdatedAtUtc = now;

            version.EstimatedReadingMinutes = command.EstimatedReadingMinutes;
            version.UpdatedAtUtc = now;

            await ClearAsync(version.Id, cancellationToken);
        }

        Fill(version, command, now);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new SaveOutcome(topic.Id, version.Status.ToString(), string.Empty, Conflict: true, null);
        }

        await ReplaceRelationshipsAsync(topic.Id, command.Relationships, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new SaveOutcome(
            topic.Id,
            version.Status.ToString(),
            Convert.ToBase64String(version.RowVersion ?? []),
            Conflict: false,
            null);
    }

    /// <summary>
    /// Everything under the version is REPLACED, not merged.
    /// </summary>
    /// <remarks>
    /// A merge would leave a section behind after the editor deleted it — a heading in the table of contents
    /// pointing at text nobody wrote. What is on screen is what is saved (ADR-0020).
    ///
    /// Deleted by QUERY, not through the change tracker. Emptying a tracked navigation whose foreign key is
    /// required tells EF to ORPHAN the children, and it then issues a DELETE for rows a previous statement
    /// already removed: "expected to affect 1 row(s), but actually affected 0". That bug is why this method
    /// looks the way it does.
    /// </remarks>
    private async Task ClearAsync(Guid versionId, CancellationToken cancellationToken)
    {
        await context.ImplementationSections
            .Where(section => context.TopicImplementations
                .Where(implementation => implementation.TopicVersionId == versionId)
                .Select(implementation => implementation.Id)
                .Contains(section.TopicImplementationId))
            .ExecuteDeleteAsync(cancellationToken);

        await context.TopicImplementations
            .Where(implementation => implementation.TopicVersionId == versionId)
            .ExecuteDeleteAsync(cancellationToken);

        await context.TopicSections
            .Where(section => section.TopicVersionId == versionId)
            .ExecuteDeleteAsync(cancellationToken);

        await context.TopicSupportedVersions
            .Where(supported => supported.TopicVersionId == versionId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private void Fill(TopicVersion version, SaveTopicCommand command, DateTime now)
    {
        foreach (var supported in command.SupportedVersions.Distinct())
        {
            context.TopicSupportedVersions.Add(new TopicSupportedVersion
            {
                Id = Guid.CreateVersion7(),
                TopicVersionId = version.Id,
                Version = supported,
            });
        }

        var order = 0;

        foreach (var section in command.Sections)
        {
            context.TopicSections.Add(new TopicSection
            {
                Id = Guid.CreateVersion7(),
                TopicVersionId = version.Id,
                SectionTypeKey = section.SectionTypeKey,
                LanguageCode = section.LanguageCode,
                Markdown = section.Markdown,
                SortOrder = order++,
                CreatedAtUtc = now,
            });
        }

        foreach (var incoming in command.Implementations)
        {
            var ecosystemId = DeterministicId.For($"ecosystem:{incoming.EcosystemKey}");

            var implementation = new TopicImplementation
            {
                Id = Guid.CreateVersion7(),
                TopicVersionId = version.Id,
                EcosystemId = ecosystemId,
                ProgrammingLanguageId = incoming.ProgrammingLanguageKey is null
                    ? null
                    : DeterministicId.For($"language:{incoming.ProgrammingLanguageKey}"),
                SupportedVersions = incoming.SupportedVersions,
                CreatedAtUtc = now,
            };

            context.TopicImplementations.Add(implementation);

            var implementationOrder = 0;

            foreach (var section in incoming.Sections)
            {
                context.ImplementationSections.Add(new ImplementationSection
                {
                    Id = Guid.CreateVersion7(),
                    TopicImplementationId = implementation.Id,
                    SectionTypeKey = section.SectionTypeKey,
                    LanguageCode = section.LanguageCode,
                    Markdown = section.Markdown,
                    SortOrder = implementationOrder++,
                });
            }
        }

        // Translations are upserted rather than replaced: the STATUS on one is a review decision, and
        // deleting the row to re-insert it would quietly reset a human's judgement to MachineDraft.
        foreach (var incoming in command.Translations)
        {
            var translation = version.Translations
                .SingleOrDefault(candidate => candidate.LanguageCode == incoming.LanguageCode);

            if (translation is null)
            {
                version.Translations.Add(new TopicTranslation
                {
                    Id = Guid.CreateVersion7(),
                    TopicVersionId = version.Id,
                    LanguageCode = incoming.LanguageCode,
                    Title = incoming.Title,
                    Summary = incoming.Summary,
                    Status = TranslationStatus.HumanDraft,
                    CreatedAtUtc = now,
                });

                continue;
            }

            translation.Title = incoming.Title;
            translation.Summary = incoming.Summary;
            translation.UpdatedAtUtc = now;
        }
    }

    /// <summary>
    /// The edges of THIS topic are replaced. Scoped, and the scope matters.
    /// </summary>
    /// <remarks>
    /// Deleting every relationship in the table and rebuilding only this topic's would be identical for a
    /// full-corpus import and catastrophic for a single save: it would erase the entire Knowledge Graph every
    /// time an editor pressed Save.
    /// </remarks>
    private async Task ReplaceRelationshipsAsync(
        Guid topicId,
        IReadOnlyList<RelationshipCommand> relationships,
        CancellationToken cancellationToken)
    {
        await context.TopicRelationships
            .Where(edge => edge.FromTopicId == topicId)
            .ExecuteDeleteAsync(cancellationToken);

        if (relationships.Count == 0) return;

        var keys = relationships.Select(relationship => relationship.ToStableKey).Distinct().ToList();

        var idOf = await context.Topics
            .Where(topic => keys.Contains(topic.StableKey))
            .ToDictionaryAsync(topic => topic.StableKey, topic => topic.Id, cancellationToken);

        foreach (var relationship in relationships)
        {
            // An edge pointing at a topic nobody wrote is a dead link in somebody's learning path — and it
            // would be discovered by a learner, months later, clicking a prerequisite that goes nowhere.
            if (!idOf.TryGetValue(relationship.ToStableKey, out var toId)) continue;
            if (toId == topicId) continue;

            context.TopicRelationships.Add(new TopicRelationship
            {
                Id = Guid.CreateVersion7(),
                FromTopicId = topicId,
                ToTopicId = toId,
                Type = Enum.Parse<RelationshipType>(relationship.Type),
            });
        }
    }

    public async Task<Result<TransitionOutcome>> TransitionAsync(
        Guid topicId,
        string toStatus,
        string? note,
        Guid reviewerId,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var target = Enum.Parse<ContentStatus>(toStatus);

        var version = await context.TopicVersions
            .Where(candidate => candidate.TopicId == topicId)
            .OrderByDescending(candidate => candidate.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (version is null)
        {
            return new Error(ErrorCodes.ResourceNotFound, "No such topic.");
        }

        if (!ContentLifecycle.MayTransition(version.Status, target))
        {
            // The transition CLAUDE.md forbids by name. A topic advances one stage at a time, and every gate
            // between a model's draft and a reader is a gate a human has to open — one at a time, on purpose.
            return Error.Validation(
                "status",
                $"{version.Status} → {target} is not a legal transition (10 § Topic Lifecycle). A topic advances "
                + "one stage at a time; it may not skip a review.");
        }

        context.TopicReviews.Add(new TopicReview
        {
            Id = Guid.CreateVersion7(),
            TopicVersionId = version.Id,
            ReviewerId = reviewerId,
            FromStatus = version.Status,
            ToStatus = target,
            Note = note,
            CreatedAtUtc = now,
        });

        version.Status = target;
        version.UpdatedAtUtc = now;

        if (target == ContentStatus.Published && version.PublishedAtUtc is null)
        {
            version.PublishedAtUtc = now;
            version.LastReviewedOn = DateOnly.FromDateTime(now);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result<TransitionOutcome>.Success(new TransitionOutcome(target.ToString()));
    }

    public async Task<TopicDraft?> DraftAsync(Guid topicId, CancellationToken cancellationToken)
    {
        var version = await context.TopicVersions
            .AsNoTracking()
            .Include(candidate => candidate.Sections)
            .Include(candidate => candidate.Implementations).ThenInclude(implementation => implementation.Sections)
            .Where(candidate => candidate.TopicId == topicId)
            .OrderByDescending(candidate => candidate.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (version is null) return null;

        return new TopicDraft(
            version.CanonicalLanguageCode,
            [
                .. version.Sections.Select(section =>
                    new SectionDraft(section.SectionTypeKey, section.LanguageCode, section.Markdown)),
                .. version.Implementations
                    .SelectMany(implementation => implementation.Sections)
                    .Select(section =>
                        new SectionDraft(section.SectionTypeKey, section.LanguageCode, section.Markdown)),
            ]);
    }

    /// <summary>
    /// Every topic, at every stage. The editor's workbench.
    /// </summary>
    /// <remarks>
    /// This is the ONE list that shows drafts, and the endpoint serving it is behind the editor roles. The
    /// reader's list (<c>ITopicRepository.ListAsync</c>) refuses them, and the two must not be merged: a
    /// single method with an <c>includeDrafts</c> flag is one forgotten argument away from serving every
    /// half-written topic in the database to the internet.
    ///
    /// It does NOT carry the prose. An editor picking a topic from a list needs a title and a status, not
    /// fifteen sections of Markdown they are not going to read.
    /// </remarks>
    public async Task<IReadOnlyList<StudioTopic>> StudioListAsync(CancellationToken cancellationToken)
    {
        var rows = await context.Topics
            .AsNoTracking()
            .OrderByDescending(topic => topic.UpdatedAtUtc ?? topic.CreatedAtUtc)
            .Select(topic => new
            {
                topic.Id,
                topic.StableKey,
                topic.Slug,
                topic.DefaultTitle,
                DomainName = topic.Domain!.Name,
                SubAreaName = topic.SubArea == null ? null : topic.SubArea.Name,
                topic.DefaultLevel,
                topic.UpdatedAtUtc,

                Version = topic.Versions
                    .OrderByDescending(version => version.VersionNumber)
                    .Select(version => new
                    {
                        version.Status,
                        Languages = version.Sections.Select(section => section.LanguageCode).Distinct().ToList(),
                        Ecosystems = version.Implementations
                            .Select(implementation => implementation.Ecosystem!.Name)
                            .ToList(),
                    })
                    .First(),
            })
            .ToListAsync(cancellationToken);

        return
        [
            .. rows.Select(row => new StudioTopic(
                row.Id,
                row.StableKey,
                row.Slug,
                row.DefaultTitle,
                row.DomainName,
                row.SubAreaName,
                row.DefaultLevel.ToString(),
                row.Version.Status.ToString(),
                row.UpdatedAtUtc,
                row.Version.Languages,
                row.Version.Ecosystems))
        ];
    }

    /// <summary>One topic, in full, for editing.</summary>
    /// <remarks>
    /// Everything, in every language, in every ecosystem — because an editor needs both languages side by
    /// side (that is how a drifted translation is noticed) and every implementation at once. The reading
    /// endpoint deliberately gives none of that.
    /// </remarks>
    public async Task<EditableTopic?> EditableAsync(Guid topicId, CancellationToken cancellationToken)
    {
        var topic = await context.Topics
            .AsNoTracking()
            .Include(candidate => candidate.Domain)
            .Include(candidate => candidate.SubArea)
            .Include(candidate => candidate.OutgoingRelationships).ThenInclude(edge => edge.ToTopic)
            .Include(candidate => candidate.Versions).ThenInclude(version => version.Sections)
            .Include(candidate => candidate.Versions).ThenInclude(version => version.Translations)
            .Include(candidate => candidate.Versions).ThenInclude(version => version.SupportedVersions)
            .Include(candidate => candidate.Versions)
                .ThenInclude(version => version.Implementations)
                .ThenInclude(implementation => implementation.Ecosystem)
            .Include(candidate => candidate.Versions)
                .ThenInclude(version => version.Implementations)
                .ThenInclude(implementation => implementation.Sections)
            .SingleOrDefaultAsync(candidate => candidate.Id == topicId, cancellationToken);

        if (topic is null) return null;

        var version = topic.Versions.OrderByDescending(candidate => candidate.VersionNumber).First();

        return new EditableTopic(
            topic.Id,
            topic.StableKey,
            topic.Slug,
            topic.Domain!.Key,
            topic.SubArea?.Key,
            topic.Category.ToString(),
            topic.DefaultLevel.ToString(),
            version.Status.ToString(),
            version.EstimatedReadingMinutes,
            version.LastReviewedOn,
            [.. version.SupportedVersions.Select(supported => supported.Version)],

            [.. version.Translations.Select(translation => new EditableTranslation(
                translation.LanguageCode,
                translation.Title,
                translation.Summary,
                translation.Status.ToString()))],

            [.. version.Sections
                .OrderBy(section => section.SortOrder)
                .Select(section => new EditableSection(
                    section.SectionTypeKey, section.LanguageCode, section.Markdown))],

            [.. version.Implementations.Select(implementation => new EditableImplementation(
                implementation.Ecosystem!.Key,
                null,
                implementation.SupportedVersions,
                [.. implementation.Sections
                    .OrderBy(section => section.SortOrder)
                    .Select(section => new EditableSection(
                        section.SectionTypeKey, section.LanguageCode, section.Markdown))]))],

            // Keys, not links. The reader gets a title and a slug because they are going to click it; the
            // editor gets the key, because they are going to change it.
            [.. topic.OutgoingRelationships.Select(edge => new EditableRelationship(
                edge.Type.ToString(),
                edge.ToTopic!.StableKey,
                edge.ToTopic.DefaultTitle))],

            Convert.ToBase64String(version.RowVersion ?? []),

            // Filled by the handler, which owns the rules. The repository does not validate — it would be a
            // second place that knows what "valid" means.
            []);
    }

    public async Task<IReadOnlyList<EditableTerm>> TermsAsync(CancellationToken cancellationToken)
    {
        var terms = await context.Terms
            .AsNoTracking()
            .Include(term => term.Explanations)
            .OrderBy(term => term.Text)
            .ToListAsync(cancellationToken);

        return
        [
            .. terms.Select(term => new EditableTerm(
                term.Id,
                term.Text,
                Split(term.Aliases),
                Split(term.ForbiddenTranslations),
                [.. term.Explanations.Select(explanation =>
                    new TermExplanationModel(explanation.LanguageCode, explanation.Text))]))
        ];
    }

    public async Task<Guid> SaveTermAsync(SaveTermCommand command, CancellationToken cancellationToken)
    {
        var term = command.Id is null
            ? null
            : await context.Terms
                .Include(candidate => candidate.Explanations)
                .SingleOrDefaultAsync(candidate => candidate.Id == command.Id, cancellationToken);

        if (term is null)
        {
            term = new Term { Id = Guid.CreateVersion7(), Text = command.Text };
            context.Terms.Add(term);
        }
        else
        {
            term.Text = command.Text;

            // Replaced, not merged. An explanation the editor deleted must disappear — otherwise it lives on
            // in the glossary, in a language nobody is maintaining.
            await context.TermExplanations
                .Where(explanation => explanation.TermId == term.Id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        term.Aliases = string.Join(';', command.Aliases);
        term.ForbiddenTranslations = string.Join(';', command.ForbiddenTranslations);

        foreach (var explanation in command.Explanations)
        {
            context.TermExplanations.Add(new TermExplanation
            {
                Id = Guid.CreateVersion7(),
                TermId = term.Id,
                LanguageCode = explanation.LanguageCode,
                Text = explanation.Text,
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        return term.Id;
    }

    public async Task<bool> DeleteTermAsync(Guid termId, CancellationToken cancellationToken)
    {
        var deleted = await context.Terms
            .Where(term => term.Id == termId)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0;
    }

    public async Task<IReadOnlyList<EditableSubArea>> SubAreasAsync(CancellationToken cancellationToken)
    {
        // The topic count travels with the theme, so the studio can say "used by 7 topics" — the number that
        // turns a refused delete into an explanation the editor can act on.
        var rows = await context.SubAreas
            .AsNoTracking()
            .OrderBy(subArea => subArea.SortOrder)
            .ThenBy(subArea => subArea.Name)
            .Select(subArea => new EditableSubArea(
                subArea.Id,
                subArea.Key,
                subArea.Name,
                context.Topics.Count(topic => topic.SubAreaId == subArea.Id)))
            .ToListAsync(cancellationToken);

        return rows;
    }

    public async Task<Guid> SaveSubAreaAsync(SaveSubAreaCommand command, CancellationToken cancellationToken)
    {
        var subArea = command.Id is null
            ? null
            : await context.SubAreas.SingleOrDefaultAsync(candidate => candidate.Id == command.Id, cancellationToken);

        if (subArea is null)
        {
            // SortOrder after the current last, so a new theme lands at the end rather than fighting for an
            // existing slot. Themes are ordered for the dropdown, not ranked.
            var nextOrder = await context.SubAreas
                .Select(existing => (int?)existing.SortOrder)
                .MaxAsync(cancellationToken) ?? 0;

            subArea = new SubArea
            {
                Id = Guid.CreateVersion7(),
                Key = command.Key,
                Name = command.Name,
                SortOrder = nextOrder + 1,
            };

            context.SubAreas.Add(subArea);
        }
        else
        {
            // The KEY is deliberately NOT updated on an edit. Tagged topics and future roadmap slices resolve
            // through it; renaming the key would orphan them exactly as renaming a topic's stable key would.
            // The display name is free to change.
            subArea.Name = command.Name;
        }

        await context.SaveChangesAsync(cancellationToken);

        return subArea.Id;
    }

    public async Task<DeleteSubAreaOutcome> DeleteSubAreaAsync(Guid subAreaId, CancellationToken cancellationToken)
    {
        // Counted BEFORE the delete, not caught as an FK violation after. A refused delete with a number is an
        // instruction ("retag these 7 first"); a caught database exception is a 500 that tells nobody anything.
        var inUse = await context.Topics.CountAsync(topic => topic.SubAreaId == subAreaId, cancellationToken);

        if (inUse > 0)
        {
            return new DeleteSubAreaOutcome(Deleted: false, InUseCount: inUse);
        }

        var deleted = await context.SubAreas
            .Where(subArea => subArea.Id == subAreaId)
            .ExecuteDeleteAsync(cancellationToken);

        return new DeleteSubAreaOutcome(Deleted: deleted > 0, InUseCount: 0);
    }

    private static IReadOnlyList<string> Split(string value) =>
        value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
