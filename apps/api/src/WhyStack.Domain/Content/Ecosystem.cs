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
/// The domain a CONCEPT belongs to: Backend, Database, Networking (ADR-0021, Decision 1).
/// </summary>
/// <remarks>
/// A topic belongs to a domain, not to a language. `Connection Pooling` is Backend — it is Backend in .NET
/// and it is Backend in Java, and the reason it exists is the same in both. That sentence is the product,
/// and until this table existed the model could not express it.
/// </remarks>
public class KnowledgeDomain
{
    public Guid Id { get; init; }

    /// <summary>"backend", "database", "networking".</summary>
    public required string Key { get; init; }

    public required string Name { get; set; }

    public int SortOrder { get; set; }
}
