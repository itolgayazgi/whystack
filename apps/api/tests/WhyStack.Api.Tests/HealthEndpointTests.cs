using System.Net;

namespace WhyStack.Api.Tests;

public class HealthEndpointTests(WhyStackApiFactory factory) : IClassFixture<WhyStackApiFactory>
{
    [Fact]
    public async Task Liveness_returns_200_and_reports_healthy()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", body);
    }

    /// <summary>
    /// The whole point of splitting liveness from readiness: liveness must not consult the database.
    /// If it did, a SQL Server outage would make the orchestrator restart every API instance — which
    /// does not fix SQL Server, and converts a degraded service into no service.
    ///
    /// This test proves it structurally rather than by wording: the readiness check is the only one
    /// registered, so if liveness ever started including it, the report would name it and this fails.
    /// </summary>
    [Fact]
    public async Task Liveness_does_not_report_any_dependency()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("sql", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Readiness_returns_200_when_the_database_is_reachable()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_route_returns_404()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
