using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WhyStack.ContentImport;
using WhyStack.Infrastructure.Persistence;

// Imports a VALIDATED content manifest into SQL Server (ADR-0018).
//
//   pnpm content:validate                       ← writes the manifest, only if the corpus passes
//   dotnet run --project apps/api/tools/WhyStack.ContentImport -- --manifest <path>
//
// The order is the gate. This program cannot import invalid content, because the manifest it needs is
// only written by a validation run that succeeded.

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

var manifestPath = configuration["manifest"];

if (string.IsNullOrWhiteSpace(manifestPath))
{
    Console.Error.WriteLine("--manifest <path> is required. Run `pnpm content:validate` first to produce it.");
    return 2;
}

if (!File.Exists(manifestPath))
{
    // The most likely reason to be here, and it is worth saying out loud rather than reporting a bare
    // "file not found": the validation run failed, so no manifest was written. That IS the mechanism
    // working — but only if the person reading this line knows it.
    Console.Error.WriteLine($"No manifest at {manifestPath}.");
    Console.Error.WriteLine("Either `pnpm content:validate` has not run, or it failed — in which case the");
    Console.Error.WriteLine("content is invalid and nothing should be imported. Read its output.");
    return 2;
}

var connectionString = configuration.GetConnectionString("WhyStackDatabase")
    ?? configuration["ConnectionStrings:WhyStackDatabase"];

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("ConnectionStrings__WhyStackDatabase is not set.");
    return 2;
}

var manifest = JsonSerializer.Deserialize<ContentManifest>(await File.ReadAllTextAsync(manifestPath))
    ?? throw new InvalidOperationException($"{manifestPath} is not a content manifest.");

var options = new DbContextOptionsBuilder<WhyStackDbContext>()
    .UseSqlServer(connectionString)
    .Options;

await using var context = new WhyStackDbContext(options);

// One transaction for the whole corpus. A partial import is worse than no import: half a Knowledge Graph
// gives a reader prerequisites that lead nowhere, and the failure is invisible until somebody clicks.
await using var transaction = await context.Database.BeginTransactionAsync();

try
{
    var importer = new ContentImporter(context, TimeProvider.System);
    var report = await importer.ImportAsync(manifest, CancellationToken.None);

    await transaction.CommitAsync();

    Console.WriteLine($"Imported {manifest.Topics.Count} topics. {report}");
    return 0;
}
catch (Exception error)
{
    await transaction.RollbackAsync();

    // Rethrown after the rollback, not swallowed into an exit code. CLAUDE.md §1.6: never hide a failure —
    // a deploy pipeline that sees "content import failed" and a stack trace can act; one that sees a bare
    // 1 cannot.
    Console.Error.WriteLine($"Import failed and was rolled back: {error.Message}");
    throw;
}
