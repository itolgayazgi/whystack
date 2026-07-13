using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WhyStack.Application.Content;

namespace WhyStack.Infrastructure.Content;

public sealed class ContentOptions
{
    public const string Section = "Content";

    /// <summary>
    /// Where <c>content/</c> is, relative to the running API or absolute.
    /// </summary>
    /// <remarks>
    /// The content ships WITH the API — it is part of the deployed artefact, not a mounted volume that can
    /// drift from the database that indexes it. A row whose <c>MarkdownPath</c> points at a file the host
    /// does not have is a topic that 404s at read time with no explanation, and the two would go out of
    /// step the first time somebody deployed one without the other.
    /// </remarks>
    public string Root { get; set; } = "content";
}

/// <summary>
/// Reads a topic's Markdown from <c>content/</c> and splits it into the sections the database indexed.
/// </summary>
/// <remarks>
/// `07` § Content Domain: "Markdown may exist in files. The database stores metadata, relationships and
/// publishing state." So the words are here, and the index is there, and this class is the seam.
///
/// The parse is the SAME rule the validator applies (`tests/content-validation`): a `## Heading` names a
/// section, spaces and hyphens are cosmetic, fenced code blocks are not headings. If the two ever
/// disagree, the database will hold a section this reader cannot find — and it will say so out loud
/// rather than serving a topic with a hole in it.
/// </remarks>
public sealed class FileSystemTopicContentReader(
    IOptions<ContentOptions> options,
    IMemoryCache cache,
    ILogger<FileSystemTopicContentReader> logger) : ITopicContentReader
{
    public Task<IReadOnlyList<TopicSectionContent>> ReadSectionsAsync(
        string markdownPath,
        string contentHash,
        IReadOnlyList<string> sectionTypes,
        CancellationToken cancellationToken)
    {
        // Keyed by the CONTENT HASH, not the path and not a timestamp.
        //
        // A cache keyed by path never expires when the file changes. A cache keyed by time expires on a
        // deploy that changed nothing, and re-reads the whole corpus for no reason. The hash is invalidated
        // by exactly one thing — the content changing — which is the only correct answer, and it is already
        // in the database because the importer put it there.
        var cached = cache.GetOrCreate($"topic-md:{contentHash}", entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromHours(1));
            return Parse(Read(markdownPath), sectionTypes, markdownPath);
        });

        return Task.FromResult(cached ?? []);
    }

    private string Read(string markdownPath)
    {
        var full = Path.Combine(options.Value.Root, RelativeToContentRoot(markdownPath));

        if (!File.Exists(full))
        {
            // Loudly. A missing file means the database and the deployed content are out of step — the row
            // says a topic exists and the words that make it a topic are not on this host. Serving an empty
            // page would hide exactly the failure somebody needs to know about (CLAUDE.md §1.6).
            throw new FileNotFoundException(
                $"The database points at {markdownPath}, which is not on this host. The content and the "
                + "database were deployed out of step.", full);
        }

        return File.ReadAllText(full);
    }

    /// <summary>
    /// The database stores <c>content/topics/…</c> — repository-relative, so a row can always be traced to
    /// the file that produced it. The deployed root may be anywhere, so the prefix is stripped and re-based.
    /// </summary>
    private static string RelativeToContentRoot(string markdownPath) =>
        markdownPath.StartsWith("content/", StringComparison.Ordinal)
            ? markdownPath["content/".Length..]
            : markdownPath;

    private IReadOnlyList<TopicSectionContent> Parse(
        string markdown,
        IReadOnlyList<string> sectionTypes,
        string markdownPath)
    {
        var bodies = new Dictionary<string, string>(StringComparer.Ordinal);

        string? current = null;
        var buffer = new List<string>();
        var inFence = false;

        void Flush()
        {
            if (current is not null)
            {
                bodies[current] = string.Join('\n', buffer).Trim();
            }

            buffer.Clear();
        }

        foreach (var line in markdown.Split('\n').Select(line => line.TrimEnd('\r')))
        {
            if (line.StartsWith("```", StringComparison.Ordinal))
            {
                inFence = !inFence;
            }

            // A `#` inside a fenced block is a comment in somebody's shell example, not a heading. Miss this
            // and a code sample quietly becomes a section.
            if (!inFence && line.StartsWith("## ", StringComparison.Ordinal))
            {
                Flush();

                // "Real-World Scenario" → "RealWorldScenario". The Markdown stays readable; the key stays
                // strict. Identical to the validator's rule, deliberately.
                current = line[3..].Trim().Replace(" ", string.Empty).Replace("-", string.Empty);
                continue;
            }

            buffer.Add(line);
        }

        Flush();

        var sections = new List<TopicSectionContent>();

        foreach (var type in sectionTypes)
        {
            if (bodies.TryGetValue(type, out var body) && body.Length > 0)
            {
                sections.Add(new TopicSectionContent(type, body));
                continue;
            }

            // The database says this section exists and the file does not have it. That means the file
            // changed without a re-import, and the topic a reader is looking at is not the topic the index
            // describes. The page still renders — an incomplete topic beats a blank one — but this is a
            // defect, and it is written down where somebody will see it.
            logger.LogWarning(
                "Topic content {Path} is missing section {Section}, which the database says it has. The "
                + "content and the database are out of step; re-run the import.",
                markdownPath,
                type);
        }

        return sections;
    }
}
