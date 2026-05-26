using FitLead.Application.Clients.BodyMetrics;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;

namespace FitLead.IntegrationTests.Features.ClientBodyMetrics;

public abstract class BodyMetricsTestBase : IntegrationTestBase
{
    protected readonly TestDb Db;
    protected readonly TestUsers Users;
    protected readonly TestApiClients Api;

    protected BodyMetricsTestBase(IntegrationTestFixture fixture) : base(fixture)
    {
        Db = new TestDb(fixture);
        Users = new TestUsers(fixture, Db);
        Api = new TestApiClients(fixture);
    }

    protected async Task<ClientMetricSetup> CreateClientWithMetricsAsync(string prefix)
    {
        var client = await Users.RegisterClientAsync(prefix);
        var metrics = await Api.BodyMetricsAsync(client.Auth);

        return new ClientMetricSetup(client.Id, metrics);
    }

    protected static async Task<ClientBodyMetricEntryDto> CreateMetricAsync(
        BodyMetricsTestClient metrics,
        DateOnly recordedAt,
        decimal weightKg = 80)
    {
        var response = await metrics.CreateAsync(
            recordedAt,
            weightKg: weightKg);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return await response.ReadRequiredJsonAsync<ClientBodyMetricEntryDto>();
    }

    protected sealed record ClientMetricSetup(
        Guid ClientId,
        BodyMetricsTestClient Metrics);
}
