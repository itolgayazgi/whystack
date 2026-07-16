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
public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("Areas");
        builder.HasKey(area => area.Id);

        builder.Property(area => area.Key).HasMaxLength(64).IsRequired();
        builder.Property(area => area.Name).HasMaxLength(128).IsRequired();

        builder.HasIndex(area => area.Key).IsUnique().HasDatabaseName("UX_Areas_Key");

        // AREAS ONLY (ADR-0027). The old KnowledgeDomains seed mixed these with lines — `backend` was an
        // area while `language` and `security` were lines inside it, so the column meant whichever the row
        // happened to be.
        builder.HasData(
            Seed("backend", "Backend", 1),
            Seed("frontend", "Frontend", 2),
            Seed("database", "Database", 3),
            Seed("devops", "DevOps", 4));
    }

    // Deterministic ids. A Guid.NewGuid() in a seed produces a DIFFERENT migration on every developer's
    // machine, and the second one to run it wipes the first one's rows.
    private static Area Seed(string key, string name, int order) => new()
    {
        Id = DeterministicId.For($"area:{key}"),
        Key = key,
        Name = name,
        SortOrder = order,
    };
}

/// <summary>
/// The eight lines of Backend (ADR-0027, from the owner's taxonomy).
/// </summary>
/// <remarks>
/// Seeded for Backend only, and deliberately: the taxonomy defines B1-B8 for Backend. Frontend, Database
/// and DevOps get their lines when their taxonomy is written — an empty area is honest; a guessed line is
/// content nobody agreed to.
/// </remarks>
public class LineConfiguration : IEntityTypeConfiguration<Line>
{
    public void Configure(EntityTypeBuilder<Line> builder)
    {
        builder.ToTable("Lines");
        builder.HasKey(line => line.Id);

        builder.Property(line => line.Key).HasMaxLength(64).IsRequired();
        builder.Property(line => line.Name).HasMaxLength(128).IsRequired();
        builder.Property(line => line.Color).HasMaxLength(7).IsRequired();

        builder.HasIndex(line => line.Key).IsUnique().HasDatabaseName("UX_Lines_Key");

        // Restrict, not Cascade. Deleting an area must not silently take eight lines and every topic on them
        // with it — the same call ADR-0023 made for a theme in use.
        builder
            .HasOne(line => line.Area)
            .WithMany()
            .HasForeignKey(line => line.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        // The colours are the taxonomy's names, resolved against the design's own palette
        // (docs/design-system/mockups/whystack-renk-sistemi-v2.html). The taxonomy names them — Altın,
        // Turuncu, Mavi… — and does not give hexes, so these are the palette's nearest member rather than
        // an invention. Every one clears WCAG's 3:1 non-text bar on the surface it is drawn on; the theme's
        // line test enforces that, and a line the reader cannot see is not a line.
        builder.HasData(
            Seed("b1-language-runtime", "Dil & Runtime", "#C9A227", 1),          // Altın — the main line
            Seed("b2-web-api", "Web API & Framework", "#C98A5A", 2),             // Turuncu
            Seed("b3-data-access", "Veri Erişimi", "#6C9BD1", 3),                // Mavi
            Seed("b4-architecture", "Mimari & Tasarım", "#A98BC9", 4),           // Mor
            Seed("b5-messaging", "Mesajlaşma & Dağıtık", "#8FBF9F", 5),          // Yeşil
            Seed("b6-security", "Güvenlik & Kimlik", "#D96A5F", 6),              // Kırmızı
            Seed("b7-testing", "Test & Kalite", "#5BB8C4", 7),                   // Turkuaz
            Seed("b8-performance", "Performans & Gözlemlenebilirlik", "#B07A4A", 8)); // Bakır
    }

    private static Line Seed(string key, string name, string color, int order) => new()
    {
        Id = DeterministicId.For($"line:{key}"),
        Key = key,
        Name = name,
        AreaId = DeterministicId.For("area:backend"),
        Color = color,
        SortOrder = order,
    };
}

public class ScopeConfiguration : IEntityTypeConfiguration<Scope>
{
    public void Configure(EntityTypeBuilder<Scope> builder)
    {
        builder.ToTable("Scopes");
        builder.HasKey(scope => scope.Id);

        builder.Property(scope => scope.Key).HasMaxLength(64).IsRequired();
        builder.Property(scope => scope.Name).HasMaxLength(128).IsRequired();

        // Unique per LINE, not globally.
        //
        // This is what lets B1's "Eşzamanlılık" and B3's "Transaction & Eşzamanlılık" both exist: a scope is
        // only meaningful inside its line (ADR-0027). A global unique key would have forced one of them to
        // be renamed to something nobody says out loud.
        builder
            .HasIndex(scope => new { scope.LineId, scope.Key })
            .IsUnique()
            .HasDatabaseName("UX_Scopes_LineId_Key");

        builder
            .HasOne(scope => scope.Line)
            .WithMany()
            .HasForeignKey(scope => scope.LineId)
            .OnDelete(DeleteBehavior.Restrict);

        // A STARTER set, not the vocabulary. Scopes are curated in the studio — these are here so the first
        // author is not staring at an empty dropdown.
        //
        // Re-parented, not re-cut (ADR-0027): these are the ADR-0023 seeds, which were never wrong — they
        // were homeless. What was missing was which line they live on.
        builder.HasData(
            Seed("async", "Async / Await", "b1-language-runtime", 1),
            Seed("memory-management", "Bellek Yönetimi", "b1-language-runtime", 2),
            Seed("collections", "Koleksiyonlar", "b1-language-runtime", 3),
            Seed("error-handling", "Hata Yönetimi", "b1-language-runtime", 4),
            Seed("concurrency", "Eşzamanlılık", "b1-language-runtime", 5),
            Seed("dependency-injection", "Dependency Injection", "b4-architecture", 1));
    }

    private static Scope Seed(string key, string name, string line, int order) => new()
    {
        // Keyed by line AND key: the id has to survive the same word appearing on two lines, which is the
        // whole point of the composite index above.
        Id = DeterministicId.For($"scope:{line}:{key}"),
        Key = key,
        Name = name,
        LineId = DeterministicId.For($"line:{line}"),
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
        // The NUMBER, not the name (ADR-0026): the basamak is ordinal, and ORDER BY on the text sorted
        // Expert before Junior in three separate queries before a test caught it.
        //
        // The CHECK is what buys back the one thing string storage gave for free. `Archetype` was stored as
        // an int by accident once and the column held 0 — a value the enum does not define — and nothing
        // complained. An int column will take any int; this one will not.
        builder.Property(topic => topic.DefaultLevel).IsRequired();

        builder.ToTable(table => table.HasCheckConstraint(
            "CK_Topics_DefaultLevel",
            "[DefaultLevel] IN (10, 20, 30, 40)"));

        // A STRING, like every other enum here. Stored as an int it was worse than ugly: the column already
        // held 0 — a value Archetype does not define — so the wire was serving "0" as a topic's type.
        builder.Property(topic => topic.Archetype).HasConversion<string>().HasMaxLength(24).IsRequired();

        // Restrict, not Cascade. Deleting a line that still has stops on it should FAIL — the topics are the
        // asset, and a foreign key is the last thing standing between a careless DELETE and the corpus.
        builder
            .HasOne(topic => topic.Line)
            .WithMany()
            .HasForeignKey(topic => topic.LineId)
            .OnDelete(DeleteBehavior.Restrict);

        // Nullable, and Restrict (ADR-0027). Null is a stop in no neighbourhood — normal. Restrict, not
        // SetNull, because deleting a scope that still tags topics would silently UNTAG them: a data loss
        // dressed as a tidy-up. The delete fails, the editor is told how many topics use it, and retags first.
        builder
            .HasOne(topic => topic.Scope)
            .WithMany()
            .HasForeignKey(topic => topic.ScopeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Owned, not three columns. `Part` without `Of` is meaningless, and a shape that can express
        // "2 of null" is a shape somebody will eventually store (ADR-0027).
        builder.OwnsOne(topic => topic.Sequence, sequence =>
        {
            sequence.Property(part => part.Group).HasColumnName("SequenceGroup").HasMaxLength(64);
            sequence.Property(part => part.Part).HasColumnName("SequencePart");
            sequence.Property(part => part.Of).HasColumnName("SequenceOf");
        });

        // "Every published stop on B3 for a Junior" is the query behind the roadmap and the topic list, and
        // it is the first one that will be slow.
        builder
            .HasIndex(topic => new { topic.LineId, topic.DefaultLevel })
            .HasDatabaseName("IX_Topics_LineId_DefaultLevel");

        // The neighbourhood cross-section — "EF Core from Junior to Senior" (ADR-0027). Filtered so the index
        // holds only tagged topics; the many nulls stay out of it.
        builder
            .HasIndex(topic => new { topic.ScopeId, topic.DefaultLevel })
            .HasFilter("[ScopeId] IS NOT NULL")
            .HasDatabaseName("IX_Topics_ScopeId_DefaultLevel");
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
