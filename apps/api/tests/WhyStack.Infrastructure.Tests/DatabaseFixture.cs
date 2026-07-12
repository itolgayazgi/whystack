using Microsoft.EntityFrameworkCore;
using WhyStack.Infrastructure.Persistence;

namespace WhyStack.Infrastructure.Tests;

/// <summary>
/// A real SQL Server, because the thing under test is SQL.
/// </summary>
/// <remarks>
/// An in-memory provider would happily accept <c>DELETE TOP (n)</c> — or quietly ignore it — and
/// <c>sp_getapplock</c> does not exist outside SQL Server at all. A prune job verified against a fake
/// database is a prune job verified against nothing.
/// </remarks>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string LocalDefault =
        "Server=127.0.0.1,1433;Database=WhyStack;User Id=sa;Password=;Encrypt=True;TrustServerCertificate=True";

    public string ConnectionString { get; } =
        Environment.GetEnvironmentVariable("ConnectionStrings__WhyStackDatabase")
        ?? throw new InvalidOperationException(
            "ConnectionStrings__WhyStackDatabase is not set. These tests need a real SQL Server — run "
                + "scripts/setup/dev-database.ps1, then set the variable, or run them in CI where it is set. "
                + $"Shape: {LocalDefault}");

    public WhyStackDbContext NewContext() =>
        new(new DbContextOptionsBuilder<WhyStackDbContext>()
            .UseSqlServer(ConnectionString)
            .Options);

    public async Task InitializeAsync()
    {
        await using var context = NewContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "database";
}
