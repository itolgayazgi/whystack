using Microsoft.EntityFrameworkCore;
using WhyStack.Application.Content.Authoring;
using WhyStack.Domain.Content;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Infrastructure.Tests;

/// <summary>
/// The theme (SubArea) axis, against a real SQL Server (ADR-0023).
/// </summary>
/// <remarks>
/// Every test runs inside a transaction that is never committed, so the shared dev database is left exactly
/// as it was found. The foreign key, the Restrict behaviour and the in-use count are SQL Server's, and an
/// in-memory provider would verify none of them.
/// </remarks>
[Collection(DatabaseCollection.Name)]
public sealed class SubAreaAuthoringTests(DatabaseFixture fixture)
{
    private const string BackendDomain = "backend";
    private const string SeededTheme = "async"; // seeded in ContentConfigurations

    private static SaveTopicCommand Topic(string suffix, string? subAreaKey) => new(
        Id: null,
        StableKey: $"backend.subarea-test-{suffix}",
        Slug: $"subarea-test-{suffix}",
        DomainKey: BackendDomain,
        SubAreaKey: subAreaKey,
        Category: "Concept",
        Level: "MidLevel",
        EstimatedReadingMinutes: 8,
        SupportedVersions: [],
        Translations: [new TranslationCommand("en", "Subarea Test", "A fixture topic.")],
        Sections: [],
        Implementations: [],
        Relationships: [],
        RowVersion: null);

    /// <summary>A valid theme tags the topic, and the tag survives a round trip.</summary>
    [Fact]
    public async Task Save_with_a_valid_theme_tags_the_topic()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);

        var outcome = await repository.SaveAsync(Topic("valid", SeededTheme), Guid.CreateVersion7(), default);

        Assert.Null(outcome.Error);

        var editable = await repository.EditableAsync(outcome.Id, default);

        Assert.Equal(SeededTheme, editable!.SubAreaKey);
    }

    /// <summary>A topic with no theme is normal — null is a fact, not an omission.</summary>
    [Fact]
    public async Task Save_with_no_theme_is_fine()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);

        var outcome = await repository.SaveAsync(Topic("none", subAreaKey: null), Guid.CreateVersion7(), default);

        Assert.Null(outcome.Error);

        var editable = await repository.EditableAsync(outcome.Id, default);

        Assert.Null(editable!.SubAreaKey);
    }

    /// <summary>
    /// A theme that does not exist is refused — the gate, not the foreign key.
    /// </summary>
    /// <remarks>
    /// The dropdown only ever offers real themes, so an unknown key arrives from a raw API call. Resolving it
    /// here turns what would be a foreign-key 500 into a 422 that names the field. If this ever passes with a
    /// created topic, the resolution has been removed and a bad key would reach the FK.
    /// </remarks>
    [Fact]
    public async Task Save_with_an_unknown_theme_is_refused_on_the_subAreaKey_field()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);

        var outcome = await repository.SaveAsync(
            Topic("unknown", "no-such-theme"), Guid.CreateVersion7(), default);

        Assert.NotNull(outcome.Error);
        Assert.Equal("subAreaKey", outcome.ErrorField);
        Assert.Equal(Guid.Empty, outcome.Id);

        // And nothing was written.
        var exists = await context.Topics.AnyAsync(topic => topic.StableKey == "backend.subarea-test-unknown");
        Assert.False(exists);
    }

    /// <summary>
    /// A theme in use cannot be deleted — deleting it would silently untag its topics.
    /// </summary>
    /// <remarks>
    /// This is the property the whole "controlled vocabulary" decision rests on. The count is returned so the
    /// editor is told what stands in the way; a bare false would be a refusal with no explanation. Untag the
    /// topic, and the same delete succeeds — proving the block is about USE, not about the theme itself.
    /// </remarks>
    [Fact]
    public async Task A_theme_in_use_cannot_be_deleted_until_its_topics_are_retagged()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);

        // A theme of our own, so the count is exactly what we tagged and not whatever the seed themes carry.
        var theme = new SubArea { Id = Guid.CreateVersion7(), Key = "subarea-test-theme", Name = "Fixture Theme", SortOrder = 900 };
        context.SubAreas.Add(theme);
        await context.SaveChangesAsync();

        var saved = await repository.SaveAsync(Topic("tagged", theme.Key), Guid.CreateVersion7(), default);
        Assert.Null(saved.Error);

        var blocked = await repository.DeleteSubAreaAsync(theme.Id, default);

        Assert.False(blocked.Deleted);
        Assert.Equal(1, blocked.InUseCount);

        // Untag the topic (a full-replacement save with no theme), then the delete goes through.
        var reload = await repository.EditableAsync(saved.Id, default);
        await repository.SaveAsync(
            Topic("tagged", subAreaKey: null) with { Id = saved.Id, RowVersion = reload!.RowVersion },
            Guid.CreateVersion7(),
            default);

        var allowed = await repository.DeleteSubAreaAsync(theme.Id, default);

        Assert.True(allowed.Deleted);
        Assert.Equal(0, allowed.InUseCount);
    }

    /// <summary>An unused theme deletes cleanly, and the count reflects reality.</summary>
    [Fact]
    public async Task An_unused_theme_deletes_and_the_count_is_zero()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);

        var theme = new SubArea { Id = Guid.CreateVersion7(), Key = "subarea-test-unused", Name = "Unused", SortOrder = 901 };
        context.SubAreas.Add(theme);
        await context.SaveChangesAsync();

        var outcome = await repository.DeleteSubAreaAsync(theme.Id, default);

        Assert.True(outcome.Deleted);
        Assert.Equal(0, outcome.InUseCount);
    }
}
