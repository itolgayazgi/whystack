using System.Text;
using System.Text.Json.Serialization;
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

// Enums cross the wire as STRINGS. `08` and CLAUDE.md §4 both forbid numeric enum values, and the
// reason is that the number is a lie that only stays true by accident: insert a member into the enum,
// every existing client silently starts meaning something else, and nothing anywhere fails.
//
// System.Text.Json defaults to numbers, so without this the API would quietly demand `platform: 0`.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

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

// The web client runs on a different origin than the API (8081 vs 5207 in development), so the browser
// will not let it call us at all without this.
//
// AllowCredentials is what makes the refresh COOKIE work: a cross-origin request only carries cookies
// when the caller sends `credentials: 'include'` AND the server answers with
// Access-Control-Allow-Credentials. And the browser flatly refuses that combination alongside a
// wildcard origin — "*" plus credentials is a spec violation, not a warning — so the origins are
// listed explicitly. That refusal is a feature: it means no deployment can accidentally let ANY
// website on the internet make authenticated calls to this API on a logged-in user's behalf.
//
// The list is configuration, because it differs per environment and a wrong entry here is a security
// bug rather than a typo.
const string WebClientCorsPolicy = "web-client";

builder.Services.AddCors(options =>
    options.AddPolicy(WebClientCorsPolicy, policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

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

// Before the rate limiter, and before authentication.
//
// A CORS preflight (OPTIONS) carries no cookie and no Authorization header — by design; that is what
// it is for. Put UseCors after the rate limiter and the preflights burn the caller's auth budget, so
// a user who simply loads the sign-in page can be rate-limited before typing anything. Put it after
// UseAuthentication and the preflight gets a 401, which the browser reports as an opaque CORS failure
// with no hint that authentication was even involved.
app.UseCors(WebClientCorsPolicy);

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
app.MapUserEndpoints();
app.MapTopicEndpoints();

app.Run();

// WebApplicationFactory<T> in WhyStack.Api.Tests needs a public entry point to bind to.
// Top-level statements generate an internal Program class, which the test project cannot see.
public partial class Program;
