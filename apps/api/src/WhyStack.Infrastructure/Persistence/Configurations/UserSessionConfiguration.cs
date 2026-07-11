using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhyStack.Domain.Identity;

namespace WhyStack.Infrastructure.Persistence.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");

        builder.HasKey(session => session.Id);

        // 64 hex characters — SHA-256. The raw token never reaches this table (ADR-0008).
        builder.Property(session => session.RefreshTokenHash).HasMaxLength(64).IsRequired();

        // Every refresh is a lookup by this hash, on what will become the largest table in the identity
        // domain. Without the index that is a full scan on the hot path of every authenticated request
        // after the access token expires — which is every fifteen minutes, per user, forever.
        //
        // Unique as well as fast: two sessions with the same token hash would mean the CSPRNG collided
        // or something is very wrong. Better to fail the insert than to find out later.
        builder
            .HasIndex(session => session.RefreshTokenHash)
            .IsUnique()
            .HasDatabaseName("UX_UserSessions_RefreshTokenHash");

        // Reuse detection revokes a whole family at once. That is a write over every row sharing the
        // FamilyId, and it happens at the exact moment you least want a table scan — while an attacker
        // is holding a live token.
        builder
            .HasIndex(session => session.FamilyId)
            .HasDatabaseName("IX_UserSessions_FamilyId");

        // "Show me my active sessions" and "revoke everything for this user".
        builder
            .HasIndex(session => new { session.UserId, session.RevokedAtUtc })
            .HasDatabaseName("IX_UserSessions_UserId_RevokedAtUtc");

        builder.Property(session => session.Platform).HasMaxLength(32);
        builder.Property(session => session.DeviceType).HasMaxLength(64);
        builder.Property(session => session.IpAddressHash).HasMaxLength(64);
        builder.Property(session => session.UserAgentHash).HasMaxLength(64);

        builder.Property(session => session.RevocationReason).HasConversion<string>().HasMaxLength(32);

        builder.Ignore(session => session.IsRevoked);
        builder.Ignore(session => session.IsRotated);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
