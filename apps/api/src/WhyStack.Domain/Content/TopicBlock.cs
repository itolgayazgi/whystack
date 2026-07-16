namespace WhyStack.Domain.Content;

/// <summary>
/// One block in a topic's flow (ADR-0024). The unit of content, of rendering, and of progress.
/// </summary>
/// <remarks>
/// A topic's body is an ORDERED SEQUENCE of these, not a fixed set of labelled sections. A block does one job
/// — a hook, a code sample, a checkpoint — and its body is JSON shaped by its <see cref="Type"/>. Both web and
/// mobile render from the same <see cref="DataJson"/> (ADR-0022); the mobile topic is the same blocks in
/// smaller bites, never a shortened one.
/// </remarks>
public class TopicBlock
{
    public Guid Id { get; init; }

    public required Guid TopicVersionId { get; init; }

    /// <summary>Position in the flow. The EN and TR sequences run in parallel — same order, different language.</summary>
    public required int Order { get; set; }

    public required BlockType Type { get; set; }

    /// <summary>Which language's copy this is. A block holds content for exactly one language.</summary>
    public required string LanguageCode { get; set; }

    /// <summary>
    /// The ecosystem this block belongs to, or null (ADR-0024).
    /// </summary>
    /// <remarks>
    /// Null means SHARED — the hook, the "why", the mental model, written once and true in every ecosystem
    /// (the promise of ADR-0021: the reason transfers). A non-null key — "dotnet", "java" — marks a block that
    /// is specific to one ecosystem: the mechanism, the code, the ecosystem's own mistakes. A reader on the
    /// .NET line sees the shared blocks plus the "dotnet" blocks, merged by <see cref="Order"/>.
    /// </remarks>
    public string? EcosystemKey { get; set; }

    /// <summary>
    /// The block's content, shaped by its type.
    /// </summary>
    /// <remarks>
    /// A checkpoint is <c>{question, options[], correct, explanation}</c>; a code block is <c>{file, lang,
    /// source, highlightLines[], annotation}</c>; a hook is <c>{question, promise}</c>. The database does not
    /// validate this shape — <c>nvarchar(max)</c> holds anything — so the shape is enforced in
    /// <c>WhyStack.Application</c> on every save (ADR-0024, Decision 6). A schemaless column with no
    /// application gate is a way to store a checkpoint with no correct answer.
    /// </remarks>
    public required string DataJson { get; set; }

    public DateTime CreatedAtUtc { get; init; }

    public TopicVersion? TopicVersion { get; init; }
}

/// <summary>
/// The twelve building blocks a topic is composed from (ADR-0024; `whystack-konu-iskeleti.md`).
/// </summary>
/// <remarks>
/// Stored as a STRING on the wire and in the column (`08`): a numeric enum value silently changes meaning the
/// day a member is inserted, in data nobody re-reads.
/// </remarks>
public enum BlockType
{
    /// <summary>Opens the topic with a QUESTION, never a definition. "Why before how" lives here (ADR-0019).</summary>
    Hook = 1,

    /// <summary>The problem story — why the thing was invented, as a short scenario.</summary>
    Story = 2,

    /// <summary>The mental model: an analogy and a precise definition.</summary>
    Concept = 3,

    /// <summary>Annotated code: source, a highlighted line, one paragraph of explanation beneath.</summary>
    Code = 4,

    /// <summary>A flow or sequence diagram (SVG).</summary>
    Diagram = 5,

    /// <summary>A comparison table that ends on a "when each one" row.</summary>
    Compare = 6,

    /// <summary>A common misconception: "X is assumed, but it is Y".</summary>
    Myth = 7,

    /// <summary>A checkpoint that breaks passive reading. Structured: options and a mandatory explanation.</summary>
    Checkpoint = 8,

    /// <summary>A production note — where it breaks in the field, at the log/metric level.</summary>
    Prod = 9,

    /// <summary>An inline term, linked to the terminology dictionary (ADR-0023).</summary>
    Term = 10,

    /// <summary>The summary: three to five "what you keep".</summary>
    Summary = 11,

    /// <summary>The next station — the continuation, and any transfer to another line.</summary>
    Next = 12,
}

/// <summary>
/// The SHAPE of a topic's explanation (ADR-0024). Decides which blocks the skeleton starts with.
/// </summary>
/// <remarks>
/// Orthogonal to <see cref="TopicCategory"/>: the archetype is HOW you teach it (a mechanism walk-through, a
/// comparison), the category is WHAT it is about (Performance, Security). "async/await gerçekte ne yapar?" is
/// a <see cref="Mechanism"/> whose category is Performance; "REST vs gRPC" is a <see cref="Comparison"/> whose
/// category is Networking. Archetype drives the editor's block skeleton; category drives discovery.
/// </remarks>
public enum Archetype
{
    /// <summary>"Why does X exist?" — hook → story → concept → code → myth → checkpoint → summary → next.</summary>
    Concept = 1,

    /// <summary>"How does X work under the hood?" — hook → concept → code×n → diagram → checkpoint → prod → …</summary>
    Mechanism = 2,

    /// <summary>"X or Y, and when?" — hook → story → compare → code×2 → checkpoint → "you decide" → …</summary>
    Comparison = 3,

    /// <summary>"What broke in the field, and why?" — hook → timeline → root cause → prod → checkpoint → …</summary>
    Incident = 4,

    /// <summary>"How do we organise this problem?" — hook → world-without → concept → diagram → code → myth → …</summary>
    Pattern = 5,

    /// <summary>"Let us build it together." — hook → step blocks (code + checkpoint) → prod → summary → next.</summary>
    Workshop = 6,
}
