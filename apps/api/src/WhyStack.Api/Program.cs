using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using WhyStack.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Infrastructure registers the DbContext and its health check. No EF Core type, no provider and no
// connection string appears in this file — the API is the composition root, not a data-access layer.
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Liveness: is this process itself alive? It runs NO dependency checks on purpose.
// If it reported the database, a SQL Server outage would make the orchestrator kill and restart every
// API instance — which does not fix SQL Server, and turns a degraded service into no service at all.
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });

// Readiness: can this instance actually serve a request right now? This is what a load balancer reads.
// A failure here means "stop sending me traffic", not "restart me".
app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions { Predicate = check => check.Tags.Contains(DependencyInjection.ReadinessTag) });

app.Run();

// WebApplicationFactory<T> in WhyStack.Api.Tests needs a public entry point to bind to.
// Top-level statements generate an internal Program class, which the test project cannot see.
public partial class Program;
