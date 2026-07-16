using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhyStack.Domain.Content;
using WhyStack.Domain.Identity;
using WhyStack.Domain.Users;

namespace WhyStack.Infrastructure.Persistence.Configurations;

/// <summary>Reading progress and the streak (ADR-0025).</summary>
public class UserTopicProgressConfiguration : IEntityTypeConfiguration<UserTopicProgress>
{
    public void Configure(EntityTypeBuilder<UserTopicProgress> builder)
    {
        builder.ToTable("UserTopicProgress");
        builder.HasKey(progress => progress.Id);

        builder.Property(progress => progress.EcosystemKey).HasMaxLength(64);

        // One row per reader per topic per LINE (ADR-0025). Without this a double-tap on "devam et" writes two
        // positions for the same station and the home screen picks whichever the query happened to order first.
        //
        // SQL Server treats NULLs as equal in a unique index, so a topic with no code (EcosystemKey null) gets
        // exactly one row too — which is what we want, and is NOT what a filtered index would give.
        builder
            .HasIndex(progress => new { progress.UserId, progress.TopicId, progress.EcosystemKey })
            .IsUnique()
            .HasDatabaseName("UX_UserTopicProgress_User_Topic_Ecosystem");

        // "Kaldığın yer" on the home screen: this reader's most recently touched station.
        builder
            .HasIndex(progress => new { progress.UserId, progress.UpdatedAtUtc })
            .HasDatabaseName("IX_UserTopicProgress_UserId_UpdatedAtUtc");

        // Cascade from the reader: deleting an account deletes their progress. It is theirs, it is not a
        // record anybody else needs, and leaving it would be personal data outliving the person (unlike
        // TopicReview, which is the evidence a human opened a gate and must survive).
        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(progress => progress.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict from the topic — two cascade paths into one table is a SQL Server error, and a topic is
        // not deleted casually anyway.
        builder
            .HasOne(progress => progress.Topic)
            .WithMany()
            .HasForeignKey(progress => progress.TopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserStreakConfiguration : IEntityTypeConfiguration<UserStreak>
{
    public void Configure(EntityTypeBuilder<UserStreak> builder)
    {
        builder.ToTable("UserStreaks");

        // The USER is the key. A surrogate id would buy nothing and allow two streaks for one person.
        builder.HasKey(streak => streak.UserId);

        builder
            .HasOne<User>()
            .WithOne()
            .HasForeignKey<UserStreak>(streak => streak.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
