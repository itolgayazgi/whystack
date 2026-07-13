using Microsoft.EntityFrameworkCore;
using WhyStack.Domain.Content;
using WhyStack.Domain.Users;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.ContentImport;

/// <summary>Writes a validated manifest into SQL Server. The database is a projection of `content/`.</summary>
public sealed class ContentImporter(WhyStackDbContext context, TimeProvider clock)
{
    public async Task<ImportReport> ImportAsync(ContentManifest manifest, CancellationToken cancellationToken)
    {
        if (manifest.SchemaVersion != 1)
        {
            // Loudly, not "best effort". A manifest from a future validator may mean something different
            // by the same field name, and importing it anyway is how content silently changes meaning.
            throw new InvalidOperationException(
                $"Manifest schema version {manifest.SchemaVersion} is not supported by this importer (expects 1).");
        }

        var report = new ImportReport();

        // PASS ONE: topics and their content. No relationships yet — an edge may point at a topic that is
        // later in this same manifest, and resolving edges while the set is half-written means the answer
        // depends on file order. Two passes; the second one sees the whole graph.
        foreach (var incoming in manifest.Topics)
        {
            await UpsertTopicAsync(incoming, report, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        // PASS TWO: the Knowledge Graph.
        await ImportRelationshipsAsync(manifest, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return report;
    }

    private async Task UpsertTopicAsync(
        ManifestTopic incoming,
        ImportReport report,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        // Translations are loaded because they are upserted field by field. Sections and supported versions
        // are NOT — they are deleted by query and rewritten.
        //
        // Not loading them is the fix, not an optimisation. Loading them puts them in a tracked navigation
        // collection, and emptying a tracked collection whose foreign key is required tells EF to ORPHAN
        // the children — which it does by issuing a DELETE for rows that `ExecuteDeleteAsync` has already
        // removed. "Expected to affect 1 row(s), but actually affected 0." Two tests found it; the app
        // would have found it on the first content edit after a deploy.
        var topic = await context.Topics
            .Include(candidate => candidate.Versions)
                .ThenInclude(version => version.Translations)
            .SingleOrDefaultAsync(candidate => candidate.StableKey == incoming.StableKey, cancellationToken);

        if (topic is null)
        {
            topic = new Topic
            {
                Id = Guid.CreateVersion7(),
                StableKey = incoming.StableKey,
                Slug = incoming.Slug,
                Technology = incoming.Technology,
                Category = Enum.Parse<TopicCategory>(incoming.Category),
                DefaultLevel = Enum.Parse<SkillLevel>(incoming.Level),
                DefaultTitle = incoming.DefaultTitle,
                CreatedAtUtc = now,
            };

            context.Topics.Add(topic);
            report.TopicsAdded++;
        }
        else
        {
            topic.Slug = incoming.Slug;
            topic.Technology = incoming.Technology;
            topic.Category = Enum.Parse<TopicCategory>(incoming.Category);
            topic.DefaultLevel = Enum.Parse<SkillLevel>(incoming.Level);
            topic.DefaultTitle = incoming.DefaultTitle;
            topic.UpdatedAtUtc = now;
        }

        var version = topic.Versions.OrderByDescending(candidate => candidate.VersionNumber).FirstOrDefault();

        if (version is null)
        {
            version = new TopicVersion
            {
                Id = Guid.CreateVersion7(),
                TopicId = topic.Id,
                VersionNumber = 1,
                Status = Enum.Parse<ContentStatus>(incoming.Status),
                CanonicalLanguageCode = incoming.CanonicalLanguage,
                MarkdownPath = incoming.CanonicalMarkdownPath,
                ContentHash = incoming.CanonicalContentHash,
                EstimatedReadingMinutes = incoming.EstimatedReadingMinutes,
                LastReviewedOn = DateOnly.Parse(incoming.LastReviewed),
                CreatedAtUtc = now,
            };

            topic.Versions.Add(version);
        }
        else if (version.ContentHash == incoming.CanonicalContentHash && version.Status.ToString() == incoming.Status)
        {
            // Nothing changed. Skipping is not just an optimisation: re-writing an unchanged row would
            // bump UpdatedAtUtc on every deploy, and "when did this topic last actually change?" would
            // stop being answerable.
            report.TopicsUnchanged++;
            return;
        }
        else
        {
            var status = Enum.Parse<ContentStatus>(incoming.Status);

            // `07`: "Published content should not be silently overwritten." The importer refuses to move a
            // status backwards past a review — that is a decision a human makes in the editorial workflow,
            // not something a file edit does on its way through a deploy pipeline.
            if (!ContentLifecycle.MayTransition(version.Status, status) && version.Status != status)
            {
                throw new InvalidOperationException(
                    $"{incoming.StableKey}: {version.Status} → {status} is not a legal transition (10 § Topic Lifecycle). "
                    + "A topic may advance one stage at a time; it may not skip a review.");
            }

            version.Status = status;
            version.MarkdownPath = incoming.CanonicalMarkdownPath;
            version.ContentHash = incoming.CanonicalContentHash;
            version.EstimatedReadingMinutes = incoming.EstimatedReadingMinutes;
            version.LastReviewedOn = DateOnly.Parse(incoming.LastReviewed);
            version.UpdatedAtUtc = now;

            if (status == ContentStatus.Published && version.PublishedAtUtc is null)
            {
                version.PublishedAtUtc = now;
            }

            report.TopicsUpdated++;
        }

        await ReplaceSectionsAsync(version, incoming, cancellationToken);
        await ReplaceSupportedVersionsAsync(version, incoming, cancellationToken);

        UpsertTranslations(version, incoming, now);
    }

    /// <summary>
    /// Sections are REPLACED, not merged.
    /// </summary>
    /// <remarks>
    /// A merge would leave a section behind after it was deleted from the Markdown — a heading the reader
    /// sees in the table of contents and cannot click, because the text it points at is gone. The file is
    /// the source of truth; the table says exactly what the file says, or it says nothing.
    ///
    /// <c>ExecuteDeleteAsync</c>, not <c>RemoveRange</c> on the navigation. The first version of this did
    /// the latter, and the tests caught it: the deleted rows STAY in <c>version.Sections</c>, so the next
    /// <c>DetectChanges</c> found them again and EF issued a second DELETE for rows that were already
    /// gone — "expected to affect 1 row(s), but actually affected 0". Deleting by query says exactly what
    /// it means, in one statement, and leaves the change tracker nothing to be clever about.
    /// </remarks>
    private async Task ReplaceSectionsAsync(
        TopicVersion version,
        ManifestTopic incoming,
        CancellationToken cancellationToken)
    {
        await context.TopicSections
            .Where(section => section.TopicVersionId == version.Id)
            .ExecuteDeleteAsync(cancellationToken);

        var order = 0;

        foreach (var key in incoming.Sections)
        {
            context.TopicSections.Add(new TopicSection
            {
                Id = Guid.CreateVersion7(),
                TopicVersionId = version.Id,
                SectionTypeKey = key,
                SortOrder = order++,
            });
        }
    }

    private async Task ReplaceSupportedVersionsAsync(
        TopicVersion version,
        ManifestTopic incoming,
        CancellationToken cancellationToken)
    {
        await context.TopicSupportedVersions
            .Where(supported => supported.TopicVersionId == version.Id)
            .ExecuteDeleteAsync(cancellationToken);

        foreach (var supported in incoming.SupportedVersions)
        {
            context.TopicSupportedVersions.Add(new TopicSupportedVersion
            {
                Id = Guid.CreateVersion7(),
                TopicVersionId = version.Id,
                Version = supported,
            });
        }
    }

    private static void UpsertTranslations(TopicVersion version, ManifestTopic incoming, DateTime now)
    {
        foreach (var incomingTranslation in incoming.Translations)
        {
            var translation = version.Translations
                .SingleOrDefault(candidate => candidate.LanguageCode == incomingTranslation.Language);

            if (translation is null)
            {
                version.Translations.Add(new TopicTranslation
                {
                    Id = Guid.CreateVersion7(),
                    TopicVersionId = version.Id,
                    LanguageCode = incomingTranslation.Language,
                    Title = incomingTranslation.Title,
                    MarkdownPath = incomingTranslation.MarkdownPath,
                    ContentHash = incomingTranslation.ContentHash,

                    // MachineDraft, and it says so. Every translation in the repository today was written
                    // by a model, and a status of Approved would be a claim that somebody read it
                    // (CLAUDE.md §1.5). A human moves it forward, in the editorial workflow, not here.
                    Status = TranslationStatus.MachineDraft,

                    CreatedAtUtc = now,
                });

                continue;
            }

            translation.Title = incomingTranslation.Title;
            translation.MarkdownPath = incomingTranslation.MarkdownPath;
            translation.ContentHash = incomingTranslation.ContentHash;
            translation.UpdatedAtUtc = now;
        }

        // A translation whose canonical text moved must SAY that it is behind. It is not deleted and it is
        // not silently served as current — `07`: "A published translation should not point to an outdated
        // canonical version without warning." The client already has a component that shows this.
        if (version.UpdatedAtUtc is not null)
        {
            foreach (var stale in version.Translations.Where(candidate =>
                candidate.Status is TranslationStatus.Approved or TranslationStatus.Published))
            {
                stale.Status = TranslationStatus.NeedsUpdate;
                stale.UpdatedAtUtc = now;
            }
        }
    }

    private async Task ImportRelationshipsAsync(ContentManifest manifest, CancellationToken cancellationToken)
    {
        var idOf = await context.Topics
            .ToDictionaryAsync(topic => topic.StableKey, topic => topic.Id, cancellationToken);

        // The graph is replaced wholesale. An edge deleted from topic.yaml must disappear from the
        // database — a merge would leave the reader a prerequisite that the author removed, and nothing
        // would ever notice. There are tens of these rows, not millions; correctness costs nothing here.
        await context.TopicRelationships.ExecuteDeleteAsync(cancellationToken);

        foreach (var topic in manifest.Topics)
        {
            foreach (var edge in topic.Relationships)
            {
                // The content validator already proved every edge resolves. If it did not, this is a bug in
                // the manifest and not something to paper over with a skip.
                if (!idOf.TryGetValue(edge.Topic, out var toId))
                {
                    throw new InvalidOperationException(
                        $"{topic.StableKey}: relationship points at \"{edge.Topic}\", which is not in the manifest.");
                }

                context.TopicRelationships.Add(new TopicRelationship
                {
                    Id = Guid.CreateVersion7(),
                    FromTopicId = idOf[topic.StableKey],
                    ToTopicId = toId,
                    Type = Enum.Parse<RelationshipType>(edge.Type),
                });
            }
        }
    }
}

public sealed class ImportReport
{
    public int TopicsAdded { get; set; }
    public int TopicsUpdated { get; set; }
    public int TopicsUnchanged { get; set; }

    public override string ToString() =>
        $"{TopicsAdded} added, {TopicsUpdated} updated, {TopicsUnchanged} unchanged.";
}
