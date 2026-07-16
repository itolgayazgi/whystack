using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WhyStack.Domain.Content;
using WhyStack.Domain.Identity;
using WhyStack.Domain.Users;

namespace WhyStack.Infrastructure.Persistence;

/// <summary>The single EF Core context for the modular monolith.</summary>
public class WhyStackDbContext(DbContextOptions<WhyStackDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserLoginEvent> UserLoginEvents => Set<UserLoginEvent>();
    public DbSet<EmailConfirmationToken> EmailConfirmationTokens => Set<EmailConfirmationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();

    public DbSet<KnowledgeDomain> KnowledgeDomains => Set<KnowledgeDomain>();
    public DbSet<SubArea> SubAreas => Set<SubArea>();
    public DbSet<Ecosystem> Ecosystems => Set<Ecosystem>();
    public DbSet<ProgrammingLanguage> ProgrammingLanguages => Set<ProgrammingLanguage>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<TopicVersion> TopicVersions => Set<TopicVersion>();
    public DbSet<TopicTranslation> TopicTranslations => Set<TopicTranslation>();
    public DbSet<TopicSection> TopicSections => Set<TopicSection>();
    public DbSet<TopicBlock> TopicBlocks => Set<TopicBlock>();
    public DbSet<TopicImplementation> TopicImplementations => Set<TopicImplementation>();
    public DbSet<ImplementationSection> ImplementationSections => Set<ImplementationSection>();
    public DbSet<TopicReview> TopicReviews => Set<TopicReview>();
    public DbSet<TopicSupportedVersion> TopicSupportedVersions => Set<TopicSupportedVersion>();
    public DbSet<TopicRelationship> TopicRelationships => Set<TopicRelationship>();
    public DbSet<SectionType> SectionTypes => Set<SectionType>();
    public DbSet<Term> Terms => Set<Term>();
    public DbSet<TermExplanation> TermExplanations => Set<TermExplanation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Entity configuration lives in one IEntityTypeConfiguration class per entity, discovered from
        // this assembly. The alternative — configuring every entity inline here — turns OnModelCreating
        // into a thousand-line file that nobody reads and everybody appends to.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WhyStackDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // SQL Server's datetime2 does NOT store the DateTimeKind. Write a UTC value, read it back, and
        // you get Kind.Unspecified — a DateTime that looks right and lies about what it is. The first
        // piece of code to call .ToLocalTime() on it then shifts it by the server's offset, and the
        // bug shows up as "the token expired three hours early" on one machine and not another.
        //
        // `07` says UTC only, and every column carries the ...Utc suffix. This is what makes that true
        // at the boundary instead of merely intended: every DateTime is tagged UTC on the way out.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }
}

/// <summary>
/// Normalises on write, tags on read.
///
/// A Local value is converted — it is unambiguous. An Unspecified value is TAGGED as UTC rather than
/// converted, because converting it would mean guessing that it was local, and a wrong guess silently
/// moves a timestamp by hours. Application code writes DateTime.UtcNow, so Unspecified should never
/// arrive; if it does, treating it as UTC is the only interpretation that does not invent an offset.
/// </summary>
internal class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    value => value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value,
    value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

internal class NullableUtcDateTimeConverter() : ValueConverter<DateTime?, DateTime?>(
    value => value.HasValue && value.Value.Kind == DateTimeKind.Local ? value.Value.ToUniversalTime() : value,
    value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value);
