namespace WhyStack.Domain.Content;

/// <summary>
/// An approved technical term (`10` § Terminology Dictionary; `content/terminology/` under ADR-0018).
/// </summary>
/// <remarks>
/// The rule this table exists for: <b>the term is preserved, the explanation is localized.</b>
///
///     Connection Pooling
///     Açılan veritabanı bağlantılarının kapatılmak yerine bir havuzda tutulup yeniden kullanılmasıdır.
///
/// The Turkish sentence is Turkish. `Connection Pooling` is still `Connection Pooling`, because that is what
/// the reader will type into a search box, read in a stack trace, and see in a job advert. A topic that
/// teaches them "bağlantı havuzlama" has taught them a word nobody else uses — and has made them worse at
/// finding the answer next time.
///
/// It moved here from a YAML file with the rest of the content (ADR-0020). It is content: an editor adds a
/// term the same way they add a topic, and the validator picks it up on the next save.
/// </remarks>
public class Term
{
    public Guid Id { get; init; }

    /// <summary>The canonical spelling. This exact string must survive translation.</summary>
    public required string Text { get; set; }

    /// <summary>
    /// Other spellings that count as the term — `DI`, `GC`, `CLR`. Semicolon-separated.
    /// </summary>
    /// <remarks>
    /// A child table would be correct and would buy nothing: nothing queries an alias, nothing indexes one,
    /// and the whole dictionary is loaded into memory to validate a single save. A join per term, to read a
    /// list of two strings, is a join for the schema diagram's benefit.
    /// </remarks>
    public string Aliases { get; set; } = string.Empty;

    /// <summary>
    /// Translations that are WRONG, listed by name. Semicolon-separated.
    /// </summary>
    /// <remarks>
    /// The half that catches the realistic failure. A translator — human or model — rarely drops a term
    /// entirely; it keeps it in the heading, where it looks diligent, and paraphrases it for the next five
    /// paragraphs. "The term survived" would pass that. Naming the paraphrase makes every occurrence
    /// checkable.
    /// </remarks>
    public string ForbiddenTranslations { get; set; } = string.Empty;

    public ICollection<TermExplanation> Explanations { get; init; } = [];
}

/// <summary>The term, explained in one language. The explanation is the only part that may be translated.</summary>
public class TermExplanation
{
    public Guid Id { get; init; }

    public required Guid TermId { get; init; }

    public required string LanguageCode { get; init; }

    public required string Text { get; set; }
}
