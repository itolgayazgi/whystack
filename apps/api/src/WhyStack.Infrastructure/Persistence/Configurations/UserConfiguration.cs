using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhyStack.Domain.Identity;

namespace WhyStack.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Email).HasMaxLength(256).IsRequired();

        builder.Property(user => user.NormalizedEmail).HasMaxLength(256).IsRequired();

        // The single most important line in this file. Without it, "the email is unique" is a comment
        // rather than a fact: two concurrent registrations both check, both find nothing, and both
        // insert. Only the database can settle that race, and it settles it here.
        //
        // Filtered so a soft-deleted account releases its address. Otherwise deleting an account would
        // permanently burn the email, and the person could never come back.
        builder
            .HasIndex(user => user.NormalizedEmail)
            .IsUnique()
            .HasFilter("[DeletedAtUtc] IS NULL")
            .HasDatabaseName("UX_Users_NormalizedEmail");

        // 256 is not a guess: PasswordHasher v3 produces a 84-character base64 string today, and this
        // leaves room for the algorithm to be upgraded underneath us without a migration.
        builder.Property(user => user.PasswordHash).HasMaxLength(256).IsRequired();

        builder.Property(user => user.DisplayName).HasMaxLength(64);

        builder.Property(user => user.RowVersion).IsRowVersion();

        builder.Ignore(user => user.IsDeleted);

        builder
            .HasMany(user => user.UserRoles)
            .WithOne(userRole => userRole.User)
            .HasForeignKey(userRole => userRole.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
