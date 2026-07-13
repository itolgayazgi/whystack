using Microsoft.EntityFrameworkCore;
using WhyStack.ContentImport;
using WhyStack.Domain.Content;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Infrastructure.Tests;

/// <summary>
/// The importer, against a real SQL Server.
/// </summary>
/// <remarks>
/// Real, because the things most likely to break are SQL's: a unique index, a foreign key to the section
/// reference table, two cascade paths into one table. An in-memory provider accepts all three and proves
/// nothing about any of them.
/// </remarks>
[Collection(DatabaseCollection.Name)]
public sealed class ContentImporterTests(DatabaseFixture database)
{
    private static readonly TimeProvider Clock = TimeProvider.System;

    [Fact]
    public async Task Imports_a_topic_with_its_sections_translations_and_versions()
    {
        var key = Key();
        await using var context = await FreshContextAsync();

        await ImportAsync(context, Manifest(Topic(key)));

        var stored = await Load(context, key);

        Assert.Equal(SlugFor(key), stored.Slug);
        Assert.Equal(TopicCategory.Concept, stored.Category);

        var version = Assert.Single(stored.Versions);
        Assert.Equal(ContentStatus.AiDraft, version.Status);
        Assert.Equal(1, version.VersionNumber);

        // The path and the hash, not the words. `07`: "Markdown may exist in files. The database stores
        // metadata, relationships and publishing state."
        Assert.Equal("content/topics/x/en.md", version.MarkdownPath);
        Assert.Equal(new string('a', 64), version.ContentHash);

        Assert.Equal(["Summary", "Definition"], version.Sections.OrderBy(s => s.SortOrder).Select(s => s.SectionTypeKey));
        Assert.Equal(["8", "9"], version.SupportedVersions.Select(v => v.Version).Order());

        var translation = Assert.Single(version.Translations);
        Assert.Equal("tr", translation.LanguageCode);

        // MachineDraft, and it matters. A model wrote it; nobody has read it. Anything further along the
        // lifecycle would be the importer making a claim on a human's behalf (CLAUDE.md §1.5).
        Assert.Equal(TranslationStatus.MachineDraft, translation.Status);
    }

    [Fact]
    public async Task Importing_the_same_manifest_twice_changes_nothing()
    {
        var key = Key();
        await using var context = await FreshContextAsync();
        var manifest = Manifest(Topic(key));

        await ImportAsync(context, manifest);

        var before = (await Load(context, key)).Versions.Single().UpdatedAtUtc;

        var report = await ImportAsync(await FreshContextAsync(), manifest);

        // Unchanged, not "updated with the same values". A no-op deploy that rewrites every row makes
        // "when did this topic last actually change?" unanswerable — and that column is the one an editor
        // uses to find content that has gone stale.
        Assert.Equal(1, report.TopicsUnchanged);
        Assert.Equal(0, report.TopicsUpdated);

        await using var after = await FreshContextAsync();
        Assert.Equal(before, (await Load(after, key)).Versions.Single().UpdatedAtUtc);
    }

    [Fact]
    public async Task Refuses_to_skip_a_review()
    {
        var key = Key();
        await using var context = await FreshContextAsync();

        await ImportAsync(context, Manifest(Topic(key) with { Status = "AiDraft" }));

        // The transition CLAUDE.md forbids by name. A file edit on its way through a deploy pipeline is not
        // a review, and this is the last place that can say so before a model's draft reaches a reader.
        var published = Manifest(Topic(key) with { Status = "Published", CanonicalContentHash = new string('b', 64) });

        await using var second = await FreshContextAsync();

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => ImportAsync(second, published));

        Assert.Contains("not a legal transition", error.Message);

        await using var after = await FreshContextAsync();
        Assert.Equal(ContentStatus.AiDraft, (await Load(after, key)).Versions.Single().Status);
    }

    [Fact]
    public async Task A_section_removed_from_the_markdown_disappears_from_the_database()
    {
        var key = Key();
        await using var context = await FreshContextAsync();

        await ImportAsync(context, Manifest(Topic(key)));

        var trimmed = Topic(key) with
        {
            Sections = ["Summary"],
            CanonicalContentHash = new string('b', 64),
        };

        await ImportAsync(await FreshContextAsync(), Manifest(trimmed));

        await using var after = await FreshContextAsync();
        var sections = (await Load(after, key)).Versions.Single().Sections;

        // A merge would leave "Definition" behind: a heading in the table of contents, pointing at text
        // the author deleted. The file is the source of truth, so the table says exactly what it says.
        Assert.Equal(["Summary"], sections.Select(section => section.SectionTypeKey));
    }

    [Fact]
    public async Task An_edge_deleted_from_topic_yaml_disappears_from_the_graph()
    {
        var from = Key();
        var to = Key();

        await using var context = await FreshContextAsync();

        var linked = Manifest(
            Topic(from) with { Relationships = [new ManifestRelationship("Next", to)] },
            Topic(to));

        await ImportAsync(context, linked);

        await using var check = await FreshContextAsync();
        Assert.Single(await EdgesFrom(check, from));

        var unlinked = Manifest(
            Topic(from) with { Relationships = [], CanonicalContentHash = new string('b', 64) },
            Topic(to));

        await ImportAsync(await FreshContextAsync(), unlinked);

        // Otherwise the reader keeps a prerequisite the author removed, and nothing ever notices — the
        // failure surfaces as a dead link in somebody's learning path, months later.
        await using var after = await FreshContextAsync();
        Assert.Empty(await EdgesFrom(after, from));
    }

    [Fact]
    public async Task An_unknown_section_type_is_refused_by_the_database_itself()
    {
        var key = Key();
        await using var context = await FreshContextAsync();

        var invented = Manifest(Topic(key) with { Sections = ["MyOwnSection"] });

        // Not caught by a check in this codebase — caught by the FOREIGN KEY to the SectionTypes reference
        // table. That is the point of ADR-0002's decision to make it a table: the blueprint is enforceable
        // by the database, so a section nobody approved cannot exist even if every layer above forgot.
        await Assert.ThrowsAnyAsync<DbUpdateException>(() => ImportAsync(context, invented));
    }

    // ── helpers ────────────────────────────────────────────────────────────────────────────────────

    private Task<WhyStackDbContext> FreshContextAsync() => Task.FromResult(database.NewContext());

    private static Task<ImportReport> ImportAsync(WhyStackDbContext context, ContentManifest manifest) =>
        new ContentImporter(context, Clock).ImportAsync(manifest, CancellationToken.None);

    private static async Task<Topic> Load(WhyStackDbContext context, string key) =>
        await context.Topics
            .Include(topic => topic.Versions).ThenInclude(version => version.Sections)
            .Include(topic => topic.Versions).ThenInclude(version => version.Translations)
            .Include(topic => topic.Versions).ThenInclude(version => version.SupportedVersions)
            .AsNoTracking()
            .SingleAsync(topic => topic.StableKey == key);

    private static async Task<List<TopicRelationship>> EdgesFrom(WhyStackDbContext context, string key)
    {
        var id = await context.Topics.Where(topic => topic.StableKey == key).Select(topic => topic.Id).SingleAsync();
        return await context.TopicRelationships.Where(edge => edge.FromTopicId == id).ToListAsync();
    }

    /// <summary>
    /// A key per test. These run against a SHARED database, and both StableKey and Slug are unique — a
    /// fixed value would make the tests pass alone and fail together, which is the worst way to find out.
    /// </summary>
    private static string Key() => $"test.{Guid.CreateVersion7():N}";

    private static string SlugFor(string key) => key.Replace('.', '-');

    private static ContentManifest Manifest(params ManifestTopic[] topics) => new(1, topics);

    private static ManifestTopic Topic(string stableKey) => new(
        StableKey: stableKey,
        Slug: SlugFor(stableKey),
        Technology: "csharp",
        Category: "Concept",
        Level: "Junior",
        DefaultTitle: "What is X?",
        Status: "AiDraft",
        LastReviewed: "2026-07-13",
        EstimatedReadingMinutes: 5,
        SupportedVersions: ["8", "9"],
        CanonicalLanguage: "en",
        CanonicalMarkdownPath: "content/topics/x/en.md",
        CanonicalContentHash: new string('a', 64),
        Sections: ["Summary", "Definition"],
        Relationships: [],
        Translations: [new ManifestTranslation("tr", "X Nedir?", "content/topics/x/tr.md", new string('c', 64))]);
}
