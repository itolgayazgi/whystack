using Microsoft.EntityFrameworkCore;

namespace WhyStack.Infrastructure.Persistence;

/// <summary>
/// The single EF Core context for the modular monolith.
/// </summary>
/// <remarks>
/// It holds no <c>DbSet</c> yet, and that is correct for Sprint 1: the roadmap defines this sprint as
/// migration infrastructure, and states that no application feature is required. Entities arrive with
/// the Identity domain in Sprint 2.
/// </remarks>
public class WhyStackDbContext(DbContextOptions<WhyStackDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Entity configuration lives in one IEntityTypeConfiguration class per entity, discovered from
        // this assembly. The alternative — configuring every entity inline here — turns OnModelCreating
        // into a thousand-line file that nobody reads and everybody appends to.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WhyStackDbContext).Assembly);
    }
}
