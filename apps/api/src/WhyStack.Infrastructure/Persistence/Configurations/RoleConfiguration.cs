using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhyStack.Domain.Identity;

namespace WhyStack.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    /// <summary>
    /// Fixed ids. The roles are part of the authorization model, not data, so they get the same id in
    /// every environment — a fixture that says "this user is an Administrator" then means the same
    /// thing in a test, on a laptop and in production. Random ids per environment turn every seeded
    /// reference into a lookup.
    /// </summary>
    private static readonly (RoleName Name, string Id, string Description)[] Seed =
    [
        (RoleName.Guest, "00000000-0000-0000-0000-000000000001", "Not signed in. May read published content."),
        (RoleName.RegisteredUser, "00000000-0000-0000-0000-000000000002", "Signed in. May learn, track progress and bookmark."),
        (RoleName.PremiumUser, "00000000-0000-0000-0000-000000000003", "Reserved for a future tier. Grants nothing today (ADR-0005, ADR-0012)."),
        (RoleName.Editor, "00000000-0000-0000-0000-000000000004", "Dormant. Will author and edit content."),
        (RoleName.Reviewer, "00000000-0000-0000-0000-000000000005", "Dormant. Will review content before publication."),
        (RoleName.Translator, "00000000-0000-0000-0000-000000000006", "Dormant. Will translate content."),
        (RoleName.Administrator, "00000000-0000-0000-0000-000000000007", "Full administrative access."),
    ];

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(role => role.Id);

        // Stored as a string, not an int. A row that reads 'Administrator' is one anybody can audit;
        // a row that reads 7 is one you have to look up — and the day someone reorders the enum, every
        // historical row silently means something else.
        builder
            .Property(role => role.Name)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(role => role.NormalizedName).HasMaxLength(32).IsRequired();

        builder
            .HasIndex(role => role.NormalizedName)
            .IsUnique()
            .HasDatabaseName("UX_Roles_NormalizedName");

        builder.Property(role => role.Description).HasMaxLength(256);

        // Seeded in the migration, not at startup. A startup seed runs on every boot, races itself
        // across instances, and is invisible in the schema history. A migration runs once, in order,
        // and is reviewable in a diff.
        builder.HasData(
            Seed.Select(role => new Role
            {
                Id = Guid.Parse(role.Id),
                Name = role.Name,
                NormalizedName = role.Name.ToString().ToUpperInvariant(),
                Description = role.Description,
                IsSystemRole = true,
                CreatedAtUtc = new DateTime(2026, 7, 12, 0, 0, 0, DateTimeKind.Utc),
            }));
    }
}
