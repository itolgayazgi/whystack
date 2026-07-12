using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using WhyStack.Api.Endpoints;
using WhyStack.Infrastructure;
using WhyStack.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Infrastructure registers the DbContext, the identity services and the use cases. No EF Core type, no
// provider and no connection string appears in this file — the API is the composition root, not a
// data-access layer.
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// Every unhandled failure leaves as Problem Details, like every handled one. `08` forbids custom error
// shapes, and an exception that escapes as raw HTML is a custom error shape nobody chose.
builder.Services.AddProblemDetails();

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Every one of these is on for a reason. Turning any of them off is a way to accept a token
            // somebody else minted.
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["SigningKey"] ?? throw new InvalidOperationException(
                    "Jwt:SigningKey is not configured. Set it in user secrets — never in appsettings."))),

            // The default is five minutes of grace on expiry. That silently turns a fifteen-minute
            // access token into a twenty-minute one, which quietly widens the window a stolen token
            // stays useful. Our clocks are not that bad.
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

// `08` requires rate limiting on authentication endpoints. Keyed by IP address: keying by email would
// let an attacker lock out any account they can name, which is a denial of service dressed as a
// defence — and keying globally would let one attacker stop everybody signing in.
//
// The limit is configuration, not a constant. Not for flexibility's sake: a test process makes every
// request from one address, so a hardcoded limit means the rate-limit test poisons every other test in
// the suite — which is exactly what happened. The limiter was right; the test design was wrong.
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy(AuthEndpoints.AuthRateLimitPolicy, context =>
    {
        // Read from RequestServices, not from builder.Configuration.
        //
        // Reading it at builder time looks identical and is wrong under WebApplicationFactory: the test
        // host injects its configuration when the host is BUILT, which is after this line would have
        // run. The app would silently keep the default, the test would set 3 and get 10, and the
        // failure — "no 429 ever arrived" — points nowhere near the cause.
        var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = configuration.GetValue("RateLimiting:Auth:PermitLimit", 10),
                Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:Auth:WindowSeconds", 60)),
                QueueLimit = 0,
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        // `08` specifies the 429 body and the Retry-After header. A 429 with no Retry-After tells the
        // client to guess, and a client that guesses retries too soon.
        context.HttpContext.Response.Headers.RetryAfter = "60";

        await Results
            .Problem(
                detail: "Too many requests were sent. Please try again later.",
                instance: context.HttpContext.Request.Path,
                statusCode: StatusCodes.Status429TooManyRequests,
                title: "Rate limit exceeded",
                type: "https://docs.whystack.dev/errors/rate-limit-exceeded",
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = "rate_limit_exceeded",
                    ["traceId"] = context.HttpContext.TraceIdentifier,
                })
            .ExecuteAsync(context.HttpContext);
    };
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Liveness: is this process itself alive? It runs NO dependency checks on purpose.
// If it reported the database, a SQL Server outage would make the orchestrator kill and restart every
// API instance — which does not fix SQL Server, and turns a degraded service into no service at all.
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });

// Readiness: can this instance actually serve a request right now? This is what a load balancer reads.
// A failure here means "stop sending me traffic", not "restart me".
app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions { Predicate = check => check.Tags.Contains(DependencyInjection.ReadinessTag) });

app.MapAuthEndpoints();

app.Run();

// WebApplicationFactory<T> in WhyStack.Api.Tests needs a public entry point to bind to.
// Top-level statements generate an internal Program class, which the test project cannot see.
public partial class Program;
