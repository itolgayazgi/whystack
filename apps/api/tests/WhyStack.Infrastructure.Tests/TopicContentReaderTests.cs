using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WhyStack.Infrastructure.Content;

namespace WhyStack.Infrastructure.Tests;

/// <summary>
/// The content reader, and the boundary it defends.
/// </summary>
/// <remarks>
/// The reader turns a string from a database row into a file read on the host's disk. That is a small
/// sentence with a large blast radius, and these tests are the fence around it.
/// </remarks>
public sealed class TopicContentReaderTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("whystack-content-").FullName;

    private FileSystemTopicContentReader Reader() => new(
        Options.Create(new ContentOptions { Root = _root }),
        new MemoryCache(new MemoryCacheOptions()),
        NullLogger<FileSystemTopicContentReader>.Instance);

    [Fact]
    public async Task Reads_the_sections_the_database_says_a_topic_has()
    {
        Write("topics/x/en.md", """
            # What is X?

            ## Summary

            The short answer.

            ## TradeOffs

            | You get | You give up |
            |---|---|
            | a | b |
            """);

        var sections = await Reader().ReadSectionsAsync(
            "content/topics/x/en.md", "hash-1", ["Summary", "TradeOffs"], CancellationToken.None);

        Assert.Equal(["Summary", "TradeOffs"], sections.Select(section => section.SectionType));
        Assert.Equal("The short answer.", sections[0].Markdown);
        Assert.Contains("You give up", sections[1].Markdown);
    }

    [Fact]
    public async Task A_hash_inside_a_code_fence_is_not_a_heading()
    {
        Write("topics/y/en.md", """
            # Shell

            ## Summary

            Run it:

            ```bash
            ## Definition
            echo "this is a comment, not a section"
            ```

            Still the summary.
            """);

        var sections = await Reader().ReadSectionsAsync(
            "content/topics/y/en.md", "hash-2", ["Summary"], CancellationToken.None);

        // Miss this and a line in somebody's shell example quietly becomes a section of the topic — and the
        // section it "ends" loses every paragraph after it.
        var summary = Assert.Single(sections);
        Assert.Contains("Still the summary.", summary.Markdown);
    }

    [Theory]
    [InlineData("content/../../../etc/passwd")]
    [InlineData("content/topics/../../../../windows/win.ini")]
    public async Task A_stored_path_that_escapes_the_content_root_is_refused(string escaping)
    {
        // THE ASSERTION THIS FILE EXISTS FOR.
        //
        // The path comes from a database row, so it is not user input — today. But `Path.Combine(root, x)`
        // silently DISCARDS the root when x is absolute, and `..` walks out of it one segment at a time.
        // Either one turns "serve a topic" into "read any file on this host", and neither looks like a bug
        // from the outside: the API returns 200 and a page.
        //
        // "The value is trusted" is not a security control. It is a description of the present.
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Reader().ReadSectionsAsync(escaping, "hash-3", ["Summary"], CancellationToken.None));

        Assert.Contains("outside the content root", error.Message);
    }

    [Fact]
    public async Task An_absolute_stored_path_is_refused()
    {
        var absolute = OperatingSystem.IsWindows() ? @"C:\windows\win.ini" : "/etc/passwd";

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Reader().ReadSectionsAsync(absolute, "hash-4", ["Summary"], CancellationToken.None));
    }

    [Fact]
    public async Task A_file_the_database_names_and_the_host_does_not_have_fails_loudly()
    {
        // Not an empty page. A missing file means the database and the deployed content are out of step,
        // and serving a blank topic would hide the one fact somebody needs (CLAUDE.md §1.6).
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            Reader().ReadSectionsAsync("content/topics/nope/en.md", "hash-5", ["Summary"], CancellationToken.None));
    }

    private void Write(string relative, string markdown)
    {
        var path = Path.Join(_root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, markdown);
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);
}
