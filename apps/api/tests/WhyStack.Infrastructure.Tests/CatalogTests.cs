using WhyStack.Application.Content.Authoring;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Infrastructure.Tests;

/// <summary>
/// The studio's dropdowns must offer keys the SAVE will accept.
/// </summary>
/// <remarks>
/// This file exists because they did not. ADR-0027 split KnowledgeDomain into Area and Line; the rename
/// moved the catalog's TYPE to <c>LineOption</c> and left its SOURCE reading the Areas table. So the "Hat"
/// dropdown offered "backend" — a key the Lines table has never contained — and every save came back
/// <c>No line "backend"</c>.
///
/// A compiler cannot see this: both tables have a Key and a Name, so the code type-checks perfectly and
/// means something else. Only a test that walks the round trip catches it — which is why the assertion here
/// is not "the catalog returns rows" but "the catalog's keys resolve".
/// </remarks>
public class CatalogTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Every_line_the_studio_offers_is_a_line_a_topic_can_be_saved_onto()
    {
        await using var context = fixture.NewContext();

        var catalog = await new TopicRepository(context).CatalogAsync(default);

        Assert.NotEmpty(catalog.Lines);

        var real = context.Lines.Select(line => line.Key).ToHashSet();

        // THE ASSERTION THIS FILE EXISTS FOR. The dropdown's keys and SaveAsync's lookup are the same table
        // or they are two products.
        Assert.All(catalog.Lines, line => Assert.Contains(line.Key, real));
    }

    [Fact]
    public async Task The_studio_is_not_offered_an_area_as_though_it_were_a_line()
    {
        await using var context = fixture.NewContext();

        var catalog = await new TopicRepository(context).CatalogAsync(default);
        var areas = context.Areas.Select(area => area.Key).ToHashSet();

        // "backend" is an area. It looks exactly like a line key — lowercase, hyphenated, sitting in a Key
        // column — and that resemblance is the whole reason the bug survived a rename and a build.
        Assert.All(catalog.Lines, line => Assert.DoesNotContain(line.Key, areas));
    }

    [Fact]
    public async Task Every_scope_the_studio_offers_names_a_line_that_exists()
    {
        await using var context = fixture.NewContext();

        var catalog = await new TopicRepository(context).CatalogAsync(default);

        Assert.NotEmpty(catalog.Scopes);

        var real = context.Lines.Select(line => line.Key).ToHashSet();

        // A scope only means something on its line (ADR-0027) — B1's "Eşzamanlılık" is not B3's "Transaction
        // & Eşzamanlılık". Without a resolvable LineKey the studio cannot tell them apart, and an author
        // would pick whichever one the list happened to show first.
        Assert.All(catalog.Scopes, scope => Assert.Contains(scope.LineKey, real));
    }

    [Fact]
    public async Task A_line_carries_the_area_it_belongs_to()
    {
        await using var context = fixture.NewContext();

        var catalog = await new TopicRepository(context).CatalogAsync(default);
        var areas = context.Areas.Select(area => area.Key).ToHashSet();

        // Eight lines is a long flat list. The author is thinking "Backend, data access" — not "b3" — so the
        // dropdown needs the area to group by, and it has to be a real one.
        Assert.All(catalog.Lines, line => Assert.Contains(line.AreaKey, areas));
    }

    [Fact]
    public async Task A_topic_saves_onto_the_first_line_the_dropdown_offers()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);
        var catalog = await new TopicRepository(context).CatalogAsync(default);

        var line = catalog.Lines[0];

        var outcome = await repository.SaveAsync(
            new SaveTopicCommand(
                Id: null,
                StableKey: $"catalog.test.{Guid.CreateVersion7():N}",
                Slug: $"catalog-test-{Guid.CreateVersion7():N}",
                LineKey: line.Key,
                ScopeKey: null,
                Category: "Concept",
                Level: "Junior",
                EstimatedReadingMinutes: 5,
                SupportedVersions: [],
                Translations: [new TranslationCommand("en", "Catalog probe", null)],
                Archetype: "Concept",
                Sequence: null,
                Blocks: [],
                Sections: [],
                Implementations: [],
                Relationships: [],
                RowVersion: null),
            Guid.CreateVersion7(),
            default);

        // THE ROUND TRIP. The studio picks a key from the catalog and hands it straight back on save; if the
        // two read different tables, this is where the reader finds out — with `No line "backend"` and no
        // idea why, because the key they chose is still on their screen.
        Assert.Null(outcome.Error);

        await transaction.RollbackAsync();
    }
}