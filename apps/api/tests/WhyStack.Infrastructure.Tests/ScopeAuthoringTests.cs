using Microsoft.EntityFrameworkCore;
using WhyStack.Application.Content.Authoring;
using WhyStack.Domain.Content;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Infrastructure.Tests;

/// <summary>
/// The theme (Scope) axis, against a real SQL Server (ADR-0023).
/// </summary>
/// <remarks>
/// Every test runs inside a transaction that is never committed, so the shared dev database is left exactly
/// as it was found. The foreign key, the Restrict behaviour and the in-use count are SQL Server's, and an
/// in-memory provider would verify none of them.
/// </remarks>
[Collection(DatabaseCollection.Name)]
public sealed class ScopeAuthoringTests(DatabaseFixture fixture)
{
        /// <summary>
    /// A real seeded LINE, not an area.
    /// </summary>
    /// <remarks>
    /// This said "backend" until ADR-0027, and it was right to: `backend` was a domain that stood in for
    /// both. Now a stop is authored onto a route, and Backend is the network the route runs through.
    /// </remarks>
    private const string TestLine = "b1-language-runtime";
    private const string SeededTheme = "async"; // seeded in ContentConfigurations

    private static SaveTopicCommand Topic(string suffix, string? subAreaKey) => new(
        Id: null,
        StableKey: $"backend.subarea-test-{suffix}",
        Slug: $"subarea-test-{suffix}",
        LineKey: TestLine,
        ScopeKey: subAreaKey,
        Category: "Concept",
        Archetype: "Concept",
        Sequence: null,
        Level: "MidLevel",
        EstimatedReadingMinutes: 8,
        SupportedVersions: [],
        Translations: [new TranslationCommand("en", "Subarea Test", "A fixture topic.")],
        Blocks: [],
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

        Assert.Equal(SeededTheme, editable!.ScopeKey);
    }

    /// <summary>
    /// An unparseable category is a clean 422, not a 500 — the studio dropdown's guarantee, made true for
    /// raw callers too.
    /// </summary>
    /// <remarks>
    /// Category is a closed enum. The editor picks it from a dropdown, so a bad value only arrives from curl.
    /// Before this guard, it reached <c>Enum.Parse</c> and threw — a 500 that blamed the server for the
    /// caller's typo. If this ever surfaces the exception again, the guard has been removed.
    /// </remarks>
    [Fact]
    public async Task Save_with_an_unknown_category_is_refused_on_the_category_field()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);

        var command = Topic("badcat", subAreaKey: null) with { Category = "Perfromance" };

        var outcome = await repository.SaveAsync(command, Guid.CreateVersion7(), default);

        Assert.NotNull(outcome.Error);
        Assert.Equal("category", outcome.ErrorField);
        Assert.Equal(Guid.Empty, outcome.Id);
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

        Assert.Null(editable!.ScopeKey);
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
        var theme = new Scope
        {
            Id = Guid.CreateVersion7(),
            Key = "scope-test-fixture",
            Name = "Fixture Scope",

            // On a real seeded line (ADR-0027): a scope only exists on one, and the composite unique index
            // is (LineId, Key) — so a fixture with no line would not even be insertable.
            LineId = DeterministicId.For("line:b1-language-runtime"),
            SortOrder = 900,
        };
        context.Scopes.Add(theme);
        await context.SaveChangesAsync();

        var saved = await repository.SaveAsync(Topic("tagged", theme.Key), Guid.CreateVersion7(), default);
        Assert.Null(saved.Error);

        var blocked = await repository.DeleteScopeAsync(theme.Id, default);

        Assert.False(blocked.Deleted);
        Assert.Equal(1, blocked.InUseCount);

        // Untag the topic (a full-replacement save with no theme), then the delete goes through.
        var reload = await repository.EditableAsync(saved.Id, default);
        await repository.SaveAsync(
            Topic("tagged", subAreaKey: null) with { Id = saved.Id, RowVersion = reload!.RowVersion },
            Guid.CreateVersion7(),
            default);

        var allowed = await repository.DeleteScopeAsync(theme.Id, default);

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

        var theme = new Scope
        {
            Id = Guid.CreateVersion7(),
            Key = "scope-test-unused",
            Name = "Unused",
            LineId = DeterministicId.For("line:b1-language-runtime"),
            SortOrder = 901,
        };
        context.Scopes.Add(theme);
        await context.SaveChangesAsync();

        var outcome = await repository.DeleteScopeAsync(theme.Id, default);

        Assert.True(outcome.Deleted);
        Assert.Equal(0, outcome.InUseCount);
    }
}
