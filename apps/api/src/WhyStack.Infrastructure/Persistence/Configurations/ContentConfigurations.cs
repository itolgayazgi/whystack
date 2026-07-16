using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhyStack.Domain.Content;
using WhyStack.Domain.Identity;

namespace WhyStack.Infrastructure.Persistence.Configurations;

/// <summary>
/// The Content domain's mapping (`07`; ADR-0020, ADR-0021).
/// </summary>
/// <remarks>
/// Enums are stored as STRINGS. `08`'s wire contract forbids a bare integer standing in for a product
/// concept, and a migration that reorders an enum turns every stored 3 into a different meaning — silently,
/// in data nobody re-reads.
/// </remarks>
public class KnowledgeDomainConfiguration : IEntityTypeConfiguration<KnowledgeDomain>
{
    public void Configure(EntityTypeBuilder<KnowledgeDomain> builder)
    {
        builder.ToTable("KnowledgeDomains");
        builder.HasKey(domain => domain.Id);

        builder.Property(domain => domain.Key).HasMaxLength(64).IsRequired();
        builder.Property(domain => domain.Name).HasMaxLength(128).IsRequired();

        builder.HasIndex(domain => domain.Key).IsUnique().HasDatabaseName("UX_KnowledgeDomains_Key");

        builder.HasData(
            Seed("backend", "Backend", 1),
            Seed("database", "Database", 2),
            Seed("language", "Language", 3),
            Seed("architecture", "Architecture", 4),
            Seed("networking", "Networking", 5),
            Seed("devops", "DevOps", 6),
            Seed("security", "Security", 7),
            Seed("testing", "Testing", 8));
    }

    // Deterministic ids. A Guid.NewGuid() in a seed produces a DIFFERENT migration on every developer's
    // machine, and the second one to run it wipes the first one's rows.
    private static KnowledgeDomain Seed(string key, string name, int order) => new()
    {
        Id = DeterministicId.For($"domain:{key}"),
        Key = key,
        Name = name,
        SortOrder = order,
    };
}

public class SubAreaConfiguration : IEntityTypeConfiguration<SubArea>
{
    public void Configure(EntityTypeBuilder<SubArea> builder)
    {
        builder.ToTable("SubAreas");
        builder.HasKey(subArea => subArea.Id);

        builder.Property(subArea => subArea.Key).HasMaxLength(64).IsRequired();
        builder.Property(subArea => subArea.Name).HasMaxLength(128).IsRequired();

        builder.HasIndex(subArea => subArea.Key).IsUnique().HasDatabaseName("UX_SubAreas_Key");

        // A STARTER set, not the vocabulary. Themes are curated in the studio (ADR-0023) — these are here so
        // the first author is not staring at an empty dropdown, and every one of them is editable and
        // deletable from the studio like any theme created there.
        builder.HasData(
            Seed("async", "Async / Await", 1),
            Seed("memory-management", "Bellek Yönetimi", 2),
            Seed("collections", "Koleksiyonlar", 3),
            Seed("error-handling", "Hata Yönetimi", 4),
            Seed("dependency-injection", "Dependency Injection", 5),
            Seed("concurrency", "Eşzamanlılık", 6));
    }

    private static SubArea Seed(string key, string name, int order) => new()
    {
        Id = DeterministicId.For($"subarea:{key}"),
        Key = key,
        Name = name,
        SortOrder = order,
    };
}

public class EcosystemConfiguration : IEntityTypeConfiguration<Ecosystem>
{
    public void Configure(EntityTypeBuilder<Ecosystem> builder)
    {
        builder.ToTable("Ecosystems");
        builder.HasKey(ecosystem => ecosystem.Id);

        builder.Property(ecosystem => ecosystem.Key).HasMaxLength(64).IsRequired();
        builder.Property(ecosystem => ecosystem.Name).HasMaxLength(128).IsRequired();

        builder.HasIndex(ecosystem => ecosystem.Key).IsUnique().HasDatabaseName("UX_Ecosystems_Key");

        // Java, Node.js and PHP are seeded but NOT available. The onboarding screen shows them as "coming
        // soon" rather than hiding them — a promise a reader can see is worth more than a shorter list —
        // and a topic must never be written against one, which is what the flag prevents.
        builder.HasData(
            Seed("dotnet", ".NET", available: true, 1),
            Seed("java", "Java", available: false, 2),
            Seed("nodejs", "Node.js", available: false, 3),
            Seed("php", "PHP", available: false, 4));
    }

    private static Ecosystem Seed(string key, string name, bool available, int order) => new()
    {
        Id = DeterministicId.For($"ecosystem:{key}"),
        Key = key,
        Name = name,
        IsAvailable = available,
        SortOrder = order,
    };
}

public class ProgrammingLanguageConfiguration : IEntityTypeConfiguration<ProgrammingLanguage>
{
    public void Configure(EntityTypeBuilder<ProgrammingLanguage> builder)
    {
        builder.ToTable("ProgrammingLanguages");
        builder.HasKey(language => language.Id);

        builder.Property(language => language.Key).HasMaxLength(64).IsRequired();
        builder.Property(language => language.Name).HasMaxLength(128).IsRequired();
        builder.Property(language => language.FenceLanguage).HasMaxLength(32).IsRequired();

        builder.HasIndex(language => language.Key).IsUnique().HasDatabaseName("UX_ProgrammingLanguages_Key");

        builder
            .HasOne(language => language.Ecosystem)
            .WithMany(ecosystem => ecosystem.Languages)
            .HasForeignKey(language => language.EcosystemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            Seed("dotnet", "csharp", "C#", "csharp", 1),
            Seed("dotnet", "fsharp", "F#", "fsharp", 2),
            Seed("java", "java", "Java", "java", 1),
            Seed("java", "kotlin", "Kotlin", "kotlin", 2),
            Seed("nodejs", "typescript", "TypeScript", "ts", 1),
            Seed("nodejs", "javascript", "JavaScript", "js", 2),
            Seed("php", "php", "PHP", "php", 1));
    }

    private static ProgrammingLanguage Seed(
        string ecosystem,
        string key,
        string name,
        string fence,
        int order) => new()
        {
            Id = DeterministicId.For($"language:{key}"),
            EcosystemId = DeterministicId.For($"ecosystem:{ecosystem}"),
            Key = key,
            Name = name,
            FenceLanguage = fence,
            SortOrder = order,
        };
}

public class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("Topics");
        builder.HasKey(topic => topic.Id);

        // The identity. Unique because everything in the graph resolves through it: two topics claiming the
        // same key are two pages the rest of the system cannot tell apart, and whichever the writer commits
        // second wins — silently.
        builder.Property(topic => topic.StableKey).HasMaxLength(128).IsRequired();
        builder.HasIndex(topic => topic.StableKey).IsUnique().HasDatabaseName("UX_Topics_StableKey");

        builder.Property(topic => topic.Slug).HasMaxLength(128).IsRequired();
        builder.HasIndex(topic => topic.Slug).IsUnique().HasDatabaseName("UX_Topics_Slug");

        builder.Property(topic => topic.DefaultTitle).HasMaxLength(256).IsRequired();
        builder.Property(topic => topic.Category).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(topic => topic.DefaultLevel).HasConversion<string>().HasMaxLength(16).IsRequired();

        // Restrict, not Cascade. Deleting a domain that still has topics in it should FAIL — the topics are
        // the asset, and a foreign key is the last thing standing between a careless DELETE and the corpus.
        builder
            .HasOne(topic => topic.Domain)
            .WithMany()
            .HasForeignKey(topic => topic.DomainId)
            .OnDelete(DeleteBehavior.Restrict);

        // Nullable, and Restrict (ADR-0023). Null is a topic with no theme — normal. Restrict, not SetNull,
        // because deleting a theme that still tags topics would silently UNTAG them: a data loss dressed as a
        // tidy-up. The delete fails, the editor is told how many topics use it, and retags them first.
        builder
            .HasOne(topic => topic.SubArea)
            .WithMany()
            .HasForeignKey(topic => topic.SubAreaId)
            .OnDelete(DeleteBehavior.Restrict);

        // "Every published Backend topic for a Junior" is the query behind the roadmap and the topic list,
        // and it is the first one that will be slow.
        builder
            .HasIndex(topic => new { topic.DomainId, topic.DefaultLevel })
            .HasDatabaseName("IX_Topics_DomainId_DefaultLevel");

        // The theme cross-section — "async from Junior to Expert" (ADR-0023, Sprint 5). Filtered so the index
        // holds only tagged topics; the many nulls stay out of it.
        builder
            .HasIndex(topic => new { topic.SubAreaId, topic.DefaultLevel })
            .HasFilter("[SubAreaId] IS NOT NULL")
            .HasDatabaseName("IX_Topics_SubAreaId_DefaultLevel");
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

        builder.Property(version => version.RowVersion).IsRowVersion();

        builder
            .HasIndex(version => new { version.TopicId, version.VersionNumber })
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
        builder.Property(translation => translation.Summary).HasMaxLength(512);
        builder.Property(translation => translation.Status).HasConversion<string>().HasMaxLength(24).IsRequired();

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
        builder.Property(section => section.LanguageCode).HasMaxLength(8).IsRequired();

        // nvarchar(max). A topic section is prose and it is the product; capping it would be a limit nobody
        // chose, discovered by an author mid-sentence.
        builder.Property(section => section.Markdown).IsRequired();

        builder
            .HasIndex(section => new { section.TopicVersionId, section.SectionTypeKey, section.LanguageCode })
            .IsUnique()
            .HasDatabaseName("UX_TopicSections_Version_Type_Language");

        // A foreign key to the reference table, not a free string. This is what makes ADR-0002's promise
        // enforceable rather than aspirational: a section type nobody approved cannot be inserted, even if
        // every layer above forgets to check.
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

public class TopicBlockConfiguration : IEntityTypeConfiguration<TopicBlock>
{
    public void Configure(EntityTypeBuilder<TopicBlock> builder)
    {
        builder.ToTable("TopicBlocks");
        builder.HasKey(block => block.Id);

        builder.Property(block => block.Type).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(block => block.LanguageCode).HasMaxLength(8).IsRequired();

        // Nullable: null is a SHARED block — the hook, the why, written once (ADR-0024). A key marks a block
        // that belongs to one ecosystem's treatment.
        builder.Property(block => block.EcosystemKey).HasMaxLength(64);

        // nvarchar(max). The block body is JSON shaped by its type, and it is the product; capping it would
        // be a limit nobody chose, discovered mid-sentence.
        builder.Property(block => block.DataJson).IsRequired();

        // One block per position, per language. Shared and ecosystem-tagged blocks share the order space —
        // the reader shows shared + the chosen ecosystem, merged by Order, so a global order per language
        // keeps the merge unambiguous.
        builder
            .HasIndex(block => new { block.TopicVersionId, block.LanguageCode, block.Order })
            .IsUnique()
            .HasDatabaseName("UX_TopicBlocks_Version_Language_Order");

        builder
            .HasOne(block => block.TopicVersion)
            .WithMany(version => version.Blocks)
            .HasForeignKey(block => block.TopicVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TopicImplementationConfiguration : IEntityTypeConfiguration<TopicImplementation>
{
    public void Configure(EntityTypeBuilder<TopicImplementation> builder)
    {
        builder.ToTable("TopicImplementations");
        builder.HasKey(implementation => implementation.Id);

        builder.Property(implementation => implementation.SupportedVersions).HasMaxLength(128).IsRequired();

        // One implementation per ecosystem per version. Two would mean the reader's `[ .NET ▾ ]` control has
        // two answers and no way to choose between them.
        builder
            .HasIndex(implementation => new { implementation.TopicVersionId, implementation.EcosystemId })
            .IsUnique()
            .HasDatabaseName("UX_TopicImplementations_TopicVersionId_EcosystemId");

        builder
            .HasOne(implementation => implementation.TopicVersion)
            .WithMany(version => version.Implementations)
            .HasForeignKey(implementation => implementation.TopicVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(implementation => implementation.Ecosystem)
            .WithMany()
            .HasForeignKey(implementation => implementation.EcosystemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<ProgrammingLanguage>()
            .WithMany()
            .HasForeignKey(implementation => implementation.ProgrammingLanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ImplementationSectionConfiguration : IEntityTypeConfiguration<ImplementationSection>
{
    public void Configure(EntityTypeBuilder<ImplementationSection> builder)
    {
        builder.ToTable("ImplementationSections");
        builder.HasKey(section => section.Id);

        builder.Property(section => section.SectionTypeKey).HasMaxLength(64).IsRequired();
        builder.Property(section => section.LanguageCode).HasMaxLength(8).IsRequired();
        builder.Property(section => section.Markdown).IsRequired();

        builder
            .HasIndex(section => new { section.TopicImplementationId, section.SectionTypeKey, section.LanguageCode })
            .IsUnique()
            .HasDatabaseName("UX_ImplementationSections_Impl_Type_Language");

        builder
            .HasOne<SectionType>()
            .WithMany()
            .HasForeignKey(section => section.SectionTypeKey)
            .HasPrincipalKey(type => type.Key)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(section => section.Implementation)
            .WithMany(implementation => implementation.Sections)
            .HasForeignKey(section => section.TopicImplementationId)
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

public class TopicReviewConfiguration : IEntityTypeConfiguration<TopicReview>
{
    public void Configure(EntityTypeBuilder<TopicReview> builder)
    {
        builder.ToTable("TopicReviews");
        builder.HasKey(review => review.Id);

        builder.Property(review => review.FromStatus).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(review => review.ToStatus).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(review => review.Note).HasMaxLength(2000);

        builder
            .HasIndex(review => review.TopicVersionId)
            .HasDatabaseName("IX_TopicReviews_TopicVersionId");

        builder
            .HasOne<TopicVersion>()
            .WithMany()
            .HasForeignKey(review => review.TopicVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict. A reviewer's account being deleted must not erase the record that they approved
        // something — that record is the only evidence a human opened the gate (CLAUDE.md §1.5).
        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(review => review.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);
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

        // Traversed in both directions: "what does this topic require" and "what requires this topic". The
        // second is how a roadmap is built, and without this index it is a scan of the whole graph.
        builder.HasIndex(relationship => relationship.ToTopicId).HasDatabaseName("IX_TopicRelationships_ToTopicId");

        builder
            .HasOne(relationship => relationship.FromTopic)
            .WithMany(topic => topic.OutgoingRelationships)
            .HasForeignKey(relationship => relationship.FromTopicId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, NOT Cascade. Two cascade paths into one table is a SQL Server error — and even if it
        // were allowed, deleting a topic should not silently delete the edges that point AT it. Those edges
        // are other topics' prerequisites; they are that topic's problem to fix, loudly, not ours to erase.
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
        builder.Property(type => type.Scope).HasConversion<string>().HasMaxLength(16).IsRequired();

        // Seeded from `10`'s Master Topic Structure, in its order (ADR-0002 Decisions 3/4), and now carrying
        // ADR-0021's classification.
        //
        // The Scope column IS the decision. `TradeOffs` is a Concept — a pool is a guess in every runtime.
        // `Syntax` is an Implementation — it is nothing BUT the language. Classify one wrong and either the
        // reasoning gets duplicated per ecosystem (the defect ADR-0021 removes) or a code sample claims to
        // be language-independent.
        builder.HasData(
            Concept("Summary", 1, mandatory: true),
            Concept("LearningObjectives", 2, mandatory: true),
            Concept("WhyThisTopicMatters", 3, mandatory: true),
            Graph("Prerequisites", 4),
            Concept("Definition", 5, mandatory: true),
            Concept("WhyItExists", 6, mandatory: true),
            Concept("ProblemItSolves", 7, mandatory: true),
            Concept("HistoricalContext", 8, mandatory: false),
            Concept("CoreMentalModel", 9, mandatory: true),
            Concept("CoreConcepts", 10, mandatory: true),

            // Implementation-scoped: these are the panel behind the `[ .NET ▾ ]` control.
            Implementation("InternalMechanics", 11),
            Implementation("Syntax", 12),
            Implementation("BasicExample", 13),
            Implementation("ProgressiveExamples", 14),

            Concept("RealWorldScenario", 15, mandatory: true),
            Concept("ArchitectureContext", 16, mandatory: false),
            Concept("PerformanceConsiderations", 17, mandatory: false),
            Concept("SecurityConsiderations", 18, mandatory: false),
            Concept("TestingConsiderations", 19, mandatory: false),
            Concept("BestPractices", 20, mandatory: true),
            Concept("CommonMistakes", 21, mandatory: true),
            Concept("TradeOffs", 22, mandatory: true),
            Concept("Alternatives", 23, mandatory: false),

            Implementation("VersionNotes", 24),

            Concept("InterviewQuestions", 25, mandatory: false),
            Concept("Quiz", 26, mandatory: false),
            Graph("RelatedTopics", 27),
            Graph("NextRecommendedTopic", 28),
            Concept("FurtherReading", 29, mandatory: false),
            Graph("Glossary", 30));
    }

    private static SectionType Concept(string key, int order, bool mandatory) => new()
    {
        Key = key,
        SortOrder = order,
        Scope = SectionScope.Concept,
        IsGraphDerived = false,
        IsMandatory = mandatory,
    };

    private static SectionType Implementation(string key, int order) => new()
    {
        Key = key,
        SortOrder = order,
        Scope = SectionScope.Implementation,
        IsGraphDerived = false,

        // Mandatory ON AN IMPLEMENTATION, not on the topic. A topic with no code — "what is a transaction?"
        // — has no implementation at all, and demanding a Syntax section from it would be demanding syntax
        // for an idea.
        IsMandatory = false,
    };

    private static SectionType Graph(string key, int order) => new()
    {
        Key = key,
        SortOrder = order,
        Scope = SectionScope.Concept,
        IsGraphDerived = true,
        IsMandatory = false,
    };
}

public class TermConfiguration : IEntityTypeConfiguration<Term>
{
    public void Configure(EntityTypeBuilder<Term> builder)
    {
        builder.ToTable("Terms");
        builder.HasKey(term => term.Id);

        builder.Property(term => term.Text).HasMaxLength(128).IsRequired();
        builder.Property(term => term.Aliases).HasMaxLength(256);
        builder.Property(term => term.ForbiddenTranslations).HasMaxLength(512);

        builder.HasIndex(term => term.Text).IsUnique().HasDatabaseName("UX_Terms_Text");
    }
}

public class TermExplanationConfiguration : IEntityTypeConfiguration<TermExplanation>
{
    public void Configure(EntityTypeBuilder<TermExplanation> builder)
    {
        builder.ToTable("TermExplanations");
        builder.HasKey(explanation => explanation.Id);

        builder.Property(explanation => explanation.LanguageCode).HasMaxLength(8).IsRequired();
        builder.Property(explanation => explanation.Text).HasMaxLength(1000).IsRequired();

        builder
            .HasIndex(explanation => new { explanation.TermId, explanation.LanguageCode })
            .IsUnique()
            .HasDatabaseName("UX_TermExplanations_TermId_LanguageCode");

        builder
            .HasOne<Term>()
            .WithMany(term => term.Explanations)
            .HasForeignKey(explanation => explanation.TermId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
