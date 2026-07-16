namespace WhyStack.Domain.Content;

/// <summary>
/// A runtime ecosystem: .NET, Java, Node.js, PHP (ADR-0021).
/// </summary>
/// <remarks>
/// An ecosystem is NOT a programming language. `.NET` is an ecosystem; `C#` and `F#` are languages inside
/// it. `10`'s Technology Hierarchy already draws the line — this is the database drawing it too.
///
/// It matters because the reader picks an ECOSYSTEM in onboarding, and what that choice selects is which
/// implementation panel opens by default. It does not filter the corpus: the concept is the same page for
/// everybody, and a reader who wants to see how Java does it can switch. That is the point of teaching the
/// reason first — the reason transfers.
/// </remarks>
public class Ecosystem
{
    public Guid Id { get; init; }

    /// <summary>Stable. Everything that points at an ecosystem points at this: "dotnet", "java", "nodejs".</summary>
    public required string Key { get; init; }

    /// <summary>What a person sees: ".NET", "Java", "Node.js".</summary>
    public required string Name { get; set; }

    /// <summary>
    /// The AREA whose network this ecosystem rebuilds (ADR-0027).
    /// </summary>
    /// <remarks>
    /// The axis means a different thing in each area, and that is the taxonomy's own table: Backend's
    /// ecosystems are languages (.NET, Java), Frontend's are frameworks (React, Vue), Database's are engines
    /// (PostgreSQL, MongoDB), DevOps' are clouds.
    ///
    /// Without this the table is flat, so `.NET` is an ecosystem of EVERYTHING — the tab strip on Frontend
    /// would offer .NET, Java and PHP. Nothing breaks today because Backend is the only area with lines,
    /// which is exactly the kind of bug that waits for the second area and then looks like a UI mistake.
    /// </remarks>
    public required Guid AreaId { get; init; }

    public Area? Area { get; init; }

    /// <summary>
    /// False for an ecosystem the product intends to cover and does not cover yet.
    /// </summary>
    /// <remarks>
    /// The onboarding screen shows Java, Node.js and PHP as "YAKINDA" — coming soon — rather than hiding
    /// them. That is a promise, and a promise a reader can see is worth more than a shorter list. But a
    /// topic must never be written against one, so the flag is here rather than only in the UI.
    /// </remarks>
    public bool IsAvailable { get; set; }

    public int SortOrder { get; set; }

    public ICollection<ProgrammingLanguage> Languages { get; init; } = [];
}

/// <summary>A language inside an ecosystem: C# in .NET, TypeScript in Node.js.</summary>
public class ProgrammingLanguage
{
    public Guid Id { get; init; }

    public required Guid EcosystemId { get; init; }

    /// <summary>"csharp", "java", "typescript".</summary>
    public required string Key { get; init; }

    /// <summary>"C#", "Java", "TypeScript".</summary>
    public required string Name { get; set; }

    /// <summary>The Markdown fence language, so a code block is highlighted correctly: `csharp`, `ts`.</summary>
    public required string FenceLanguage { get; set; }

    public int SortOrder { get; set; }

    public Ecosystem? Ecosystem { get; init; }
}

/// <summary>
/// An AREA of engineering: Backend, Frontend, Database, DevOps (ADR-0027).
/// </summary>
/// <remarks>
/// The outermost thing a topic belongs to, and it is not a language. `Connection Pooling` is Backend — it
/// is Backend in .NET and it is Backend in Java, and the reason it exists is the same in both (ADR-0021).
///
/// Was <c>Area</c>, and it held two different ideas at once: `backend` was an area while
/// `language` and `security` were LINES inside it, so the same column answered a different question
/// depending on the row. ADR-0027 split them. The rename to Area is part of that: this codebase's innermost
/// layer is already called Domain, and a "Area" that no longer means what it says, next to a
/// <c>WhyStack.Domain</c> that means something else, is two traps for the price of one.
/// </remarks>
public class Area
{
    public Guid Id { get; init; }

    /// <summary>"backend", "frontend", "database", "devops".</summary>
    public required string Key { get; init; }

    public required string Name { get; set; }

    public int SortOrder { get; set; }
}

/// <summary>
/// A LINE through an area: B1 Dil &amp; Runtime, B3 Veri Erişimi (ADR-0027).
/// </summary>
/// <remarks>
/// The map's line, and the thing an ECOSYSTEM is not. Selecting Java does not add a line beside .NET — it
/// rebuilds the same eight lines in Java. The ecosystem is which network you are looking at; the line is a
/// route through it. The content model already worked this way (ADR-0021: the concept is written once, only
/// the implementation slice changes), so this is the metaphor catching up with the data.
/// </remarks>
public class Line
{
    public Guid Id { get; init; }

    /// <summary>Stable: "b1-language-runtime", "b3-data-access".</summary>
    public required string Key { get; init; }

    /// <summary>What the map's legend shows: "Dil &amp; Runtime".</summary>
    public required string Name { get; set; }

    public required Guid AreaId { get; init; }

    /// <summary>
    /// The line's colour on the map, as a hex string.
    /// </summary>
    /// <remarks>
    /// DATA, not a token — and this is the one place that is right. A line is a row an editor can add, so
    /// its colour cannot live in a TypeScript file the editor has no access to; the palette a colour must be
    /// drawn FROM is still the design system's (CLAUDE.md §1.8), and the seed uses it.
    /// </remarks>
    public required string Color { get; set; }

    public int SortOrder { get; set; }

    public Area? Area { get; init; }
}

/// <summary>
/// A SCOPE — a neighbourhood of 3-10 stops on a line: EF Core, Async / Await (ADR-0027).
/// </summary>
/// <remarks>
/// Was <c>Scope</c> (ADR-0023). One axis, one name: the owner's kapsam design and ADR-0023's theme axis
/// turned out to be the same idea described twice, and two names for one thing is how they both stay alive.
///
/// <b>Metadata, never a menu level.</b> EF Core is not one stop and it is not a tier of navigation — it is
/// eight stops spread across zones (Junior 2, Mid 4, Senior 2), drawn as a bracket over the line. Making it
/// a tier would take the reader from four taps to six.
///
/// <b>A scope is 3-10 stops.</b> Fewer and it is a stop, not a neighbourhood; more and it splits in two.
///
/// It lives on a LINE, and that is what lets the same word mean two things honestly: B1's "Eşzamanlılık" is
/// the language's threads and locks; B3's "Transaction &amp; Eşzamanlılık" is isolation levels and
/// deadlocks. Two neighbourhoods, two lines — not a duplicate to be tidied away.
///
/// A controlled vocabulary, curated in the studio, because its entire purpose is grouping: `async` vs
/// `asenkron` vs `asynchrony` would split the neighbourhood in silence.
/// </remarks>
public class Scope
{
    public Guid Id { get; init; }

    /// <summary>Stable. Every tagged topic and every roadmap slice groups on this: "ef-core", "async".</summary>
    public required string Key { get; init; }

    /// <summary>What an editor and a reader see: "EF Core", "Async / Await".</summary>
    public required string Name { get; set; }

    public required Guid LineId { get; init; }

    public int SortOrder { get; set; }

    public Line? Line { get; init; }
}
