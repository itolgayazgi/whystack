using WhyStack.Domain.Content;

namespace WhyStack.Infrastructure.Persistence;

/// <summary>
/// Which stops a line has, and the order they stand in. <b>One fact, one home.</b>
/// </summary>
/// <remarks>
/// Two callers read this: the line map, which draws the stops, and the reader, which says "durak 4/12" and
/// links to the stop before this one. They are the same claim about the same line — so if each wrote its own
/// ORDER BY, the day somebody adjusted one, the map would draw a route the reader contradicts, and nothing
/// would fail. A quiet disagreement between two screens is worse than an outage: an outage gets reported.
/// </remarks>
internal static class LineOrder
{
    /// <summary>
    /// The stops a reader is actually offered: published, and nothing else.
    /// </summary>
    /// <remarks>
    /// A draft counted here would shift every number after it — a signed-in editor would read "durak 5/13"
    /// where an anonymous visitor on the same stop reads "durak 4/12", and the stop before would be a 404 for
    /// one of them. Status lives on the newest VERSION, not the topic, so this asks the version.
    /// </remarks>
    public static IQueryable<Topic> Published(this IQueryable<Topic> topics) =>
        topics.Where(topic =>
            topic.Versions
                .OrderByDescending(version => version.VersionNumber)
                .Select(version => version.Status)
                .First() == ContentStatus.Published);

    /// <summary>
    /// The station ORDER.
    /// </summary>
    /// <remarks>
    /// Level first — the basamak the whole product is built on — then the theme's own order (ADR-0023), then
    /// the title, which is what makes the sequence STABLE rather than merely correct. Deliberately NOT a walk
    /// over the `Next` edges: those are sparse today, and a topological sort over a sparse graph produces a
    /// confident-looking order that reshuffles the moment an editor adds one edge. A wrong-but-stable line is
    /// honest; a line that reshuffles under the reader is not.
    ///
    /// <c>Scope</c> is nullable and that is normal — a standalone stop belongs to no neighbourhood. SQL Server
    /// sorts NULL first ascending, so those stops cluster at the head of their level: arbitrary, but stable,
    /// which is the property that matters here.
    /// </remarks>
    public static IOrderedQueryable<Topic> InLineOrder(this IQueryable<Topic> topics) =>
        topics
            .OrderBy(topic => topic.DefaultLevel)
            .ThenBy(topic => topic.Scope!.SortOrder)
            .ThenBy(topic => topic.DefaultTitle);
}
