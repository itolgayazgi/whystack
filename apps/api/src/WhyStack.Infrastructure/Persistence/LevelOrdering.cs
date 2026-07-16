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
/// like a line — and it runs Expert-first. It shipped in two queries before a test that seeded a Junior
/// topic named "Aaa" and an Expert named "Zzz" caught it. Sorting by title alone would have looked correct
/// there, which is the trap.
///
/// The CASE is written out because EF Core has to translate it. A helper method returning an int would
/// compile, and then throw at runtime — or worse, silently pull every topic into memory to sort them.
/// </remarks>
internal static class LevelOrdering
{
    public static IOrderedQueryable<Topic> OrderByLevel(this IQueryable<Topic> topics) =>
        topics.OrderBy(topic =>
            topic.DefaultLevel == SkillLevel.Junior ? 0
            : topic.DefaultLevel == SkillLevel.MidLevel ? 1
            : topic.DefaultLevel == SkillLevel.Senior ? 2
            : 3);
}
