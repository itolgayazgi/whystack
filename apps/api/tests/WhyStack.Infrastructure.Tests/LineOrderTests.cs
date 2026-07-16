using Microsoft.EntityFrameworkCore;
using WhyStack.Application.Content.Authoring;
using WhyStack.Domain.Content;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Infrastructure.Tests;

/// <summary>
/// The line map and the reader must walk the SAME route.
/// </summary>
/// <remarks>
/// The map draws the stops; the reader says "durak 4/12" and links to the stop before this one. Those are two
/// screens making the same claim about one line. They read <see cref="LineOrder"/> for exactly that reason —
/// and this file is what stops the two drifting apart again.
///
/// <b>Why an equality assertion and not "the reader returns stops":</b> a divergence here does not throw and
/// does not look broken. The map would draw a route the reader quietly contradicts, "önceki durak" would land
/// on a stop that is not before this one, and every screen involved would still render beautifully. Nobody
/// files that bug — they just stop trusting the numbers.
///
/// Every test runs inside a transaction that is never committed, so the shared dev database is left exactly as
/// it was found.
/// </remarks>
[Collection(DatabaseCollection.Name)]
public sealed class LineOrderTests(DatabaseFixture fixture)
{
    /// <summary>
    /// A line of this test's OWN, created and rolled back with it.
    /// </summary>
    /// <remarks>
    /// This said "b1-language-runtime" first, and three of the four tests failed the moment the whole solution
    /// ran: <c>WhyStack.Api.Tests</c> is a separate PROCESS that COMMITS published stops onto b1, so "durak 1"
    /// was really durak 7 and "önceki durak" was an apitest fixture. Rolling back my own writes is not enough
    /// when the assertion is about a shared route — position is a claim about every row on the line, including
    /// the ones another process is committing while this one reads.
    ///
    /// So the line is created here, inside the transaction. Nothing else can put a stop on a line that does
    /// not exist yet, which is what makes the numbers below mean something.
    /// </remarks>
    private const string TestLine = "test-line-order";

    private const string TestEcosystem = "dotnet";

    /// <summary>
    /// Seeded so that LEVEL and TITLE pull in OPPOSITE directions.
    /// </summary>
    /// <remarks>
    /// <b>This fixture is the test.</b> The first version of it read A-junior, B-junior, C-mid, D-senior — so
    /// level-then-title and title-alone produced the same sequence, and sabotaging the ordering to a bare
    /// <c>OrderBy(DefaultTitle)</c> left all four assertions green. A fixture that cannot tell the right
    /// answer from the wrong one is a fixture that tests nothing, however much it asserts.
    ///
    /// So: the Senior sorts FIRST by title and must come LAST by route; the Juniors sort LAST by title and
    /// must come FIRST. Any ordering that is not level-first now produces a visibly different sequence. The
    /// two Juniors tie on level and share a null scope, which leaves the title as the only thing separating
    /// them — the tiebreaker that turns a merely correct order into a stable one.
    ///
    /// Insertion order is a third order again, so a query that forgot to sort at all also fails here.
    /// </remarks>
    private static readonly (string Suffix, string Level, string Title)[] Seeded =
    [
        ("m", "MidLevel", "M — mid"),
        ("a", "Senior", "A — senior"),
        ("z", "Junior", "Z — junior"),
        ("y", "Junior", "Y — junior"),
    ];

    /// <summary>
    /// The route these stops make: level first (ADR-0026's spacing), then title.
    /// </summary>
    /// <remarks>
    /// Deliberately the REVERSE of the title order (a, m, y, z) at every position. That is what makes a wrong
    /// ordering visible rather than plausible.
    /// </remarks>
    private static readonly string[] ExpectedRoute =
        ["line-order-y", "line-order-z", "line-order-m", "line-order-a"];

    private static SaveTopicCommand Topic(string suffix, string level, string title) => new(
        Id: null,
        StableKey: $"backend.line-order-{suffix}",
        Slug: $"line-order-{suffix}",
        LineKey: TestLine,
        ScopeKey: null,
        Category: "Concept",
        Archetype: "Concept",
        Level: level,
        EstimatedReadingMinutes: 8,
        SupportedVersions: [],
        Translations: [new TranslationCommand("en", title, "A fixture stop.")],
        Blocks: [],
        Sections: [],
        Implementations: [],
        Relationships: [],
        RowVersion: null);

    /// <summary>
    /// Seeds the four stops and PUBLISHES them.
    /// </summary>
    /// <remarks>
    /// The status is set on the row rather than walked through the review lifecycle. That is not a shortcut
    /// around a rule — the lifecycle has its own tests — it is keeping this file about the one thing it
    /// claims: the order. A draft is invisible to both callers, so without this the "route" would be empty
    /// and every assertion below would pass by testing nothing.
    /// </remarks>
    private static async Task<Dictionary<Guid, string>> SeedAsync(WhyStackDbContext context)
    {
        // The line first. Any seeded area will do — this test is about the order of stops on a line, not about
        // which network the line runs through.
        var areaId = await context.Areas.Select(area => area.Id).FirstAsync();

        context.Lines.Add(new Line
        {
            Id = Guid.CreateVersion7(),
            Key = TestLine,
            Name = "Test — line order",
            AreaId = areaId,
            Color = "#C9A227",
            SortOrder = 999,
        });

        await context.SaveChangesAsync();

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);
        var slugOf = new Dictionary<Guid, string>();

        foreach (var (suffix, level, title) in Seeded)
        {
            var outcome = await repository.SaveAsync(
                Topic(suffix, level, title), Guid.CreateVersion7(), default);

            Assert.Null(outcome.Error);
            slugOf[outcome.Id] = $"line-order-{suffix}";
        }

        await context.TopicVersions
            .Where(version => slugOf.Keys.Contains(version.TopicId))
            .ExecuteUpdateAsync(set => set.SetProperty(version => version.Status, ContentStatus.Published));

        return slugOf;
    }

    [Fact]
    public async Task The_map_and_the_reader_walk_the_same_route()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var slugOf = await SeedAsync(context);

        var readersRoute = await new TopicRepository(context).StopsOnLineAsync(TestLine, default);

        var map = await new RoadmapRepository(context).GetAsync(
            Guid.CreateVersion7(), TestEcosystem, TestLine, "en", default);

        Assert.NotNull(map);

        // Narrowed to the seeded stops on BOTH sides, so this compares RELATIVE order rather than depending on
        // the dev database being empty. Two sequences that agree on every pair agree on the route.
        var reader = readersRoute
            .Select(id => slugOf.GetValueOrDefault(id))
            .OfType<string>()
            .ToArray();

        var drawn = map.Stations
            .Select(station => station.Slug)
            .Where(slug => slugOf.ContainsValue(slug))
            .ToArray();

        // THE ASSERTION THIS FILE EXISTS FOR.
        Assert.Equal(drawn, reader);

        // …and neither is merely self-consistent. Both agreeing on the WRONG order is a failure this catches
        // and a plain "they match" would not.
        Assert.Equal(ExpectedRoute, reader);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task The_first_stop_has_nothing_before_it_and_the_last_has_nothing_after()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        await SeedAsync(context);

        var repository = new TopicRepository(context);
        var handler = new WhyStack.Application.Content.GetTopicHandler(repository);

        var first = await handler.HandleAsync("line-order-y", "en", null, false, default);
        var last = await handler.HandleAsync("line-order-a", "en", null, false, default);

        // Null, not a link that loops to itself and not a link back to the catalogue. The first stop of a line
        // has nothing before it — that is a fact about the route, and the UI says something different for it.
        Assert.Null(first.Value!.Stop!.Previous);
        Assert.Null(last.Value!.Stop!.Next);

        Assert.Equal("line-order-z", first.Value.Stop.Next!.Slug);
        Assert.Equal("line-order-m", last.Value.Stop.Previous!.Slug);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task A_stop_knows_where_it_stands()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        await SeedAsync(context);

        var handler = new WhyStack.Application.Content.GetTopicHandler(new TopicRepository(context));

        var third = await handler.HandleAsync("line-order-m", "en", null, false, default);

        // 1-based, because the reader counts "durak 3/4", not "durak 2/4". The design's chip says so.
        Assert.Equal(3, third.Value!.Stop!.Position);

        // Exact, because the line is this test's own. An earlier version asserted `Total >= 4` to tolerate
        // stops another process had committed — which is a test shrugging at the number it exists to check.
        Assert.Equal(Seeded.Length, third.Value.Stop.Total);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task A_draft_is_not_on_the_route()
    {
        await using var context = fixture.NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        await SeedAsync(context);

        var repository = new ContentAuthoringRepository(context, TimeProvider.System);
        var outcome = await repository.SaveAsync(
            Topic("draft", "Junior", "AA — a draft"), Guid.CreateVersion7(), default);

        Assert.Null(outcome.Error);

        var handler = new WhyStack.Application.Content.GetTopicHandler(new TopicRepository(context));

        // An editor previewing their own draft. Junior + a title that sorts before both Juniors, so if drafts
        // leaked into the route it would take position 1 and push every published stop down one — the editor
        // and the anonymous visitor would read different numbers for the same stop, and "önceki durak" would
        // 404 for one of them.
        var draft = await handler.HandleAsync("line-order-draft", "en", null, mayReadDrafts: true, default);

        Assert.NotNull(draft.Value);
        Assert.Null(draft.Value.Stop);

        // And the published route is untouched by its existence.
        var first = await handler.HandleAsync("line-order-y", "en", null, false, default);
        Assert.Equal(1, first.Value!.Stop!.Position);

        await transaction.RollbackAsync();
    }
}
