using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhyStack.Domain.Content;

namespace WhyStack.Infrastructure.Persistence.Configurations;

/// <summary>
/// The Content domain's mapping (`07` — Content, Topic Versioning and Localization domains).
/// </summary>
/// <remarks>
/// Enums are stored as STRINGS. `07`'s naming rules and `08`'s wire contract both forbid a bare integer
/// standing in for a product concept — and a migration that reorders an enum turns every stored 3 into a
/// different meaning, silently, in data nobody re-reads.
/// </remarks>
public class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("Topics");
        builder.HasKey(topic => topic.Id);

        // The identity. Unique because everything in the graph resolves through it: two topics claiming
        // the same key are two pages the rest of the system cannot tell apart, and whichever the importer
        // writes second wins — silently.
        builder.Property(topic => topic.StableKey).HasMaxLength(128).IsRequired();
        builder.HasIndex(topic => topic.StableKey).IsUnique().HasDatabaseName("UX_Topics_StableKey");

        // The URL. Every topic page is a lookup by this, so it is the one index a reader actually waits on.
        builder.Property(topic => topic.Slug).HasMaxLength(128).IsRequired();
        builder.HasIndex(topic => topic.Slug).IsUnique().HasDatabaseName("UX_Topics_Slug");

        builder.Property(topic => topic.Technology).HasMaxLength(64).IsRequired();
        builder.Property(topic => topic.DefaultTitle).HasMaxLength(256).IsRequired();

        builder.Property(topic => topic.Category).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(topic => topic.DefaultLevel).HasConversion<string>().HasMaxLength(16).IsRequired();

        // "Every published C# topic for a Junior" is the query behind the roadmap and the topic list, and
        // it is the first one that will be slow. Covering it here costs nothing now and a migration later.
        builder
            .HasIndex(topic => new { topic.Technology, topic.DefaultLevel })
            .HasDatabaseName("IX_Topics_Technology_DefaultLevel");
    }
}

public class TopicVersionConfiguration : IEntityTypeConfiguration<TopicVersion>
{
    public void Configure(EntityTypeBuilder<TopicVersion> builder)
    {
        builder.ToTable("TopicVersions");
        builder.HasKey(version => version.Id);

        builder.Property(version => version.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(version => version.CanonicalLanguageCode).HasMaxLength(8).IsRequired();

        // The pointer back to content/, and the bytes it held when it was imported. `07` names both:
        // "MarkdownPath links the database record to repository content. ContentHash helps detect file
        // changes." The hash is also what a response can be cached against — content, not clock.
        builder.Property(version => version.MarkdownPath).HasMaxLength(512).IsRequired();
        builder.Property(version => version.ContentHash).HasMaxLength(64).IsRequired();

        builder.Property(version => version.RowVersion).IsRowVersion();

        builder.HasIndex(version => new { version.TopicId, version.VersionNumber })
            .IsUnique()
            .HasDatabaseName("UX_TopicVersions_TopicId_VersionNumber");

        builder
            .HasOne(version => version.Topic)
            .WithMany(topic => topic.Versions)
            .HasForeignKey(version => version.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TopicTranslationConfiguration : IEntityTypeConfiguration<TopicTranslation>
{
    public void Configure(EntityTypeBuilder<TopicTranslation> builder)
    {
        builder.ToTable("TopicTranslations");
        builder.HasKey(translation => translation.Id);

        builder.Property(translation => translation.LanguageCode).HasMaxLength(8).IsRequired();
        builder.Property(translation => translation.Title).HasMaxLength(256).IsRequired();
        builder.Property(translation => translation.MarkdownPath).HasMaxLength(512).IsRequired();
        builder.Property(translation => translation.ContentHash).HasMaxLength(64).IsRequired();
        builder.Property(translation => translation.Status).HasConversion<string>().HasMaxLength(24).IsRequired();

        // One translation per language per version — and this is the index the language resolver hits on
        // every topic request, twice: once for the language the reader asked for, once for the fallback.
        builder
            .HasIndex(translation => new { translation.TopicVersionId, translation.LanguageCode })
            .IsUnique()
            .HasDatabaseName("UX_TopicTranslations_TopicVersionId_LanguageCode");

        builder
            .HasOne(translation => translation.TopicVersion)
            .WithMany(version => version.Translations)
            .HasForeignKey(translation => translation.TopicVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TopicSectionConfiguration : IEntityTypeConfiguration<TopicSection>
{
    public void Configure(EntityTypeBuilder<TopicSection> builder)
    {
        builder.ToTable("TopicSections");
        builder.HasKey(section => section.Id);

        builder.Property(section => section.SectionTypeKey).HasMaxLength(64).IsRequired();

        builder
            .HasIndex(section => new { section.TopicVersionId, section.SectionTypeKey })
            .IsUnique()
            .HasDatabaseName("UX_TopicSections_TopicVersionId_SectionTypeKey");

        // A foreign key to the reference table, not a free string. This is what makes ADR-0002's promise
        // enforceable rather than aspirational: a section type that is not in the blueprint cannot be
        // inserted, and one that IS in the blueprint can never be dropped without the database saying so.
        builder
            .HasOne<SectionType>()
            .WithMany()
            .HasForeignKey(section => section.SectionTypeKey)
            .HasPrincipalKey(type => type.Key)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(section => section.TopicVersion)
            .WithMany(version => version.Sections)
            .HasForeignKey(section => section.TopicVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TopicSupportedVersionConfiguration : IEntityTypeConfiguration<TopicSupportedVersion>
{
    public void Configure(EntityTypeBuilder<TopicSupportedVersion> builder)
    {
        builder.ToTable("TopicSupportedVersions");
        builder.HasKey(supported => supported.Id);

        builder.Property(supported => supported.Version).HasMaxLength(32).IsRequired();

        builder
            .HasIndex(supported => new { supported.TopicVersionId, supported.Version })
            .IsUnique()
            .HasDatabaseName("UX_TopicSupportedVersions_TopicVersionId_Version");

        builder
            .HasOne<TopicVersion>()
            .WithMany(version => version.SupportedVersions)
            .HasForeignKey(supported => supported.TopicVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TopicRelationshipConfiguration : IEntityTypeConfiguration<TopicRelationship>
{
    public void Configure(EntityTypeBuilder<TopicRelationship> builder)
    {
        builder.ToTable("TopicRelationships");
        builder.HasKey(relationship => relationship.Id);

        builder.Property(relationship => relationship.Type).HasConversion<string>().HasMaxLength(24).IsRequired();

        builder
            .HasIndex(relationship => new { relationship.FromTopicId, relationship.ToTopicId, relationship.Type })
            .IsUnique()
            .HasDatabaseName("UX_TopicRelationships_From_To_Type");

        // Traversed in both directions: "what does this topic require" and "what requires this topic".
        // The second is how a roadmap is built, and without this index it is a scan of the whole graph.
        builder.HasIndex(relationship => relationship.ToTopicId).HasDatabaseName("IX_TopicRelationships_ToTopicId");

        builder
            .HasOne(relationship => relationship.FromTopic)
            .WithMany(topic => topic.OutgoingRelationships)
            .HasForeignKey(relationship => relationship.FromTopicId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, NOT Cascade. Two cascade paths into one table is a "may cause cycles or multiple
        // cascade paths" error from SQL Server — and even if it were allowed, deleting a topic should not
        // silently delete the edges that point AT it. Those edges are other topics' prerequisites; they
        // are that topic's problem to fix, loudly, not ours to erase quietly.
        builder
            .HasOne(relationship => relationship.ToTopic)
            .WithMany()
            .HasForeignKey(relationship => relationship.ToTopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SectionTypeConfiguration : IEntityTypeConfiguration<SectionType>
{
    public void Configure(EntityTypeBuilder<SectionType> builder)
    {
        builder.ToTable("SectionTypes");

        // The KEY is the key. A surrogate id here would buy nothing and cost a join on every section.
        builder.HasKey(type => type.Key);
        builder.Property(type => type.Key).HasMaxLength(64);

        // Seeded from `10`'s Master Topic Structure, in its order (ADR-0002, Decision 3/4). A section in
        // the blueprint that is missing here is ADDED — never dropped. This is the whole reason it is a
        // table: the next educational section is a row, not a migration and an argument.
        builder.HasData(
            Seed("Summary", 1),
            Seed("LearningObjectives", 2),
            Seed("WhyThisTopicMatters", 3),
            Seed("Prerequisites", 4, graphDerived: true),
            Seed("Definition", 5),
            Seed("WhyItExists", 6),
            Seed("ProblemItSolves", 7),
            Seed("HistoricalContext", 8),
            Seed("CoreMentalModel", 9),
            Seed("CoreConcepts", 10),
            Seed("InternalMechanics", 11),
            Seed("Syntax", 12),
            Seed("BasicExample", 13),
            Seed("ProgressiveExamples", 14),
            Seed("RealWorldScenario", 15),
            Seed("ArchitectureContext", 16),
            Seed("PerformanceConsiderations", 17),
            Seed("SecurityConsiderations", 18),
            Seed("TestingConsiderations", 19),
            Seed("BestPractices", 20),
            Seed("CommonMistakes", 21),
            Seed("TradeOffs", 22),
            Seed("Alternatives", 23),
            Seed("VersionNotes", 24),
            Seed("InterviewQuestions", 25),
            Seed("Quiz", 26),
            Seed("RelatedTopics", 27, graphDerived: true),
            Seed("NextRecommendedTopic", 28, graphDerived: true),
            Seed("FurtherReading", 29),
            Seed("Glossary", 30, graphDerived: true));
    }

    private static SectionType Seed(string key, int sortOrder, bool graphDerived = false) =>
        new() { Key = key, SortOrder = sortOrder, IsGraphDerived = graphDerived };
}
