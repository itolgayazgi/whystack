var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.Run();

// WebApplicationFactory<T> in WhyStack.Api.Tests needs a public entry point to bind to.
// Top-level statements generate an internal Program class, which the test project cannot see.
public partial class Program;
