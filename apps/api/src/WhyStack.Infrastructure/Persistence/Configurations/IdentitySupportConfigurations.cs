using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhyStack.Domain.Identity;
using WhyStack.Domain.Users;

namespace WhyStack.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        // Composite key. It is also the constraint that makes "assign this role" idempotent: a second
        // attempt fails at the database rather than creating a duplicate grant nobody can revoke,
        // because revoking one row leaves the other.
        builder.HasKey(userRole => new { userRole.UserId, userRole.RoleId });

        builder
            .HasOne(userRole => userRole.Role)
            .WithMany()
            .HasForeignKey(userRole => userRole.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserLoginEventConfiguration : IEntityTypeConfiguration<UserLoginEvent>
{
    public void Configure(EntityTypeBuilder<UserLoginEvent> builder)
    {
        builder.ToTable("UserLoginEvents");

        builder.HasKey(loginEvent => loginEvent.Id);

        builder.Property(loginEvent => loginEvent.Email).HasMaxLength(256).IsRequired();

        builder
            .Property(loginEvent => loginEvent.EventType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(loginEvent => loginEvent.FailureReason).HasMaxLength(128);
        builder.Property(loginEvent => loginEvent.IpAddressHash).HasMaxLength(64);
        builder.Property(loginEvent => loginEvent.UserAgentHash).HasMaxLength(64);

        // The two questions this table exists to answer, and the shapes they are asked in:
        // "is this account under attack" (by email, recently) and "what happened to this user".
        //
        // Descending on time because every one of those questions is about the recent past. An
        // ascending index makes the database read the whole history to reach the end of it.
        builder
            .HasIndex(loginEvent => new { loginEvent.Email, loginEvent.CreatedAtUtc })
            .IsDescending(false, true)
            .HasDatabaseName("IX_UserLoginEvents_Email_CreatedAtUtc");

        builder
            .HasIndex(loginEvent => new { loginEvent.UserId, loginEvent.CreatedAtUtc })
            .IsDescending(false, true)
            .HasDatabaseName("IX_UserLoginEvents_UserId_CreatedAtUtc");

        // No FK to Users on purpose. An audit row must survive the account it describes — including a
        // failed login against an address that never had one. A cascade here would delete the evidence
        // along with the account, which is the opposite of what an audit log is for.
    }
}

public class EmailConfirmationTokenConfiguration : IEntityTypeConfiguration<EmailConfirmationToken>
{
    public void Configure(EntityTypeBuilder<EmailConfirmationToken> builder)
    {
        builder.ToTable("EmailConfirmationTokens");

        builder.HasKey(token => token.Id);

        builder.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(token => token.Email).HasMaxLength(256).IsRequired();

        builder
            .HasIndex(token => token.TokenHash)
            .IsUnique()
            .HasDatabaseName("UX_EmailConfirmationTokens_TokenHash");

        builder.Ignore(token => token.IsUsed);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");

        builder.HasKey(token => token.Id);

        builder.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();

        builder
            .HasIndex(token => token.TokenHash)
            .IsUnique()
            .HasDatabaseName("UX_PasswordResetTokens_TokenHash");

        builder.Ignore(token => token.IsUsed);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserPreferencesConfiguration : IEntityTypeConfiguration<UserPreferences>
{
    public void Configure(EntityTypeBuilder<UserPreferences> builder)
    {
        builder.ToTable("UserPreferences");

        builder.HasKey(preferences => preferences.Id);

        // One row per user, enforced by the database. A second preferences row is not a bug you notice
        // — it is a bug where half your settings changes go somewhere nobody reads.
        builder
            .HasIndex(preferences => preferences.UserId)
            .IsUnique()
            .HasDatabaseName("UX_UserPreferences_UserId");

        builder.Property(preferences => preferences.ApplicationLanguageCode).HasMaxLength(8).IsRequired();
        builder.Property(preferences => preferences.ContentLanguageCode).HasMaxLength(8).IsRequired();

        builder.Property(preferences => preferences.ThemeMode).HasConversion<string>().HasMaxLength(16);
        // ADR-0026: the number. NULL is a real answer — a reader who has not told us — so the CHECK has to
        // permit it explicitly. `IN (...)` alone is UNKNOWN for NULL, which SQL Server treats as passing,
        // but writing it out means the next person does not have to remember that.
        builder.Property(preferences => preferences.PreferredSkillLevel);

        builder.ToTable(table => table.HasCheckConstraint(
            "CK_UserPreferences_PreferredSkillLevel",
            "[PreferredSkillLevel] IS NULL OR [PreferredSkillLevel] IN (10, 20, 30, 40)"));

        builder.Property(preferences => preferences.RowVersion).IsRowVersion();

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(preferences => preferences.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
