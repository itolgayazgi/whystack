using WhyStack.Domain.Content;
using WhyStack.Domain.Users;

namespace WhyStack.Infrastructure.Persistence;

/// <summary>
/// Orders topics up the basamak: Junior → MidLevel → Senior → Expert.
/// </summary>
/// <remarks>
/// <b>Never <c>OrderBy(topic => topic.DefaultLevel)</c>.</b> SkillLevel is persisted with
/// <c>HasConversion&lt;string&gt;()</c>, so the database sorts the TEXT, and the text sorts
/// <c>Expert, Junior, MidLevel, Senior</c> — alphabetical, and almost exactly backwards. The enum's numeric
/// order only exists in C#; the moment the sort happens in SQL, it is gone.
///
/// It fails silently, which is why it is worth a file: the query succeeds, the page renders, the line looks
/// like a line — and it runs Expert-first. It was in THREE queries before a test that seeded a Junior topic
/// named "Aaa" and an Expert named "Zzz" caught one of them. Sorting by title alone would have looked
/// correct there, which is the trap — and is why the seed data in that test is what it is.
///
/// The CASE is written out because EF Core has to translate it. A helper method returning an int would
/// compile, and then throw at runtime — or worse, silently pull every topic into memory to sort them.
/// </remarks>
internal static class LevelOrdering
{
    public static IOrderedQueryable<Topic> OrderByLevel(this IQueryable<Topic> topics) =>
        topics.OrderBy(Rank);

    public static IOrderedQueryable<Topic> ThenByLevel(this IOrderedQueryable<Topic> topics) =>
        topics.ThenBy(Rank);

    /// <summary>
    /// The rung, as an expression rather than a method.
    /// </summary>
    /// <remarks>
    /// An <c>Expression</c> field, not <c>private static int Rank(Topic)</c>: EF Core translates expression
    /// TREES, and a compiled method is opaque to it. The method version compiles and then either throws or
    /// — depending on the version and the query shape — evaluates on the client, quietly loading the table.
    /// </remarks>
    private static readonly System.Linq.Expressions.Expression<Func<Topic, int>> Rank =
        topic =>
            topic.DefaultLevel == SkillLevel.Junior ? 0
            : topic.DefaultLevel == SkillLevel.MidLevel ? 1
            : topic.DefaultLevel == SkillLevel.Senior ? 2
            : 3;
}
