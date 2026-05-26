using System.Net;
using FitLead.Application.Clients.BodyMetrics;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.ClientBodyMetrics;

public sealed class BodyMetricUpdateDeleteTests(IntegrationTestFixture fixture)
    : BodyMetricsTestBase(fixture)
{
    [Fact]
    public async Task ClientCanUpdateOwnEntry()
    {
        var setup = await CreateClientWithMetricsAsync("body-metrics-update");
        var entry = await CreateMetricAsync(
            setup.Metrics,
            new DateOnly(2026, 5, 20),
            weightKg: 80);

        var response = await setup.Metrics.UpdateAsync(
            entry.Id,
            new DateOnly(2026, 5, 21),
            weightKg: 79.25m,
            waistCm: 82.5m,
            note: "Updated");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.ReadRequiredJsonAsync<ClientBodyMetricEntryDto>();
        dto.Id.Should().Be(entry.Id);
        dto.RecordedAt.Should().Be(new DateOnly(2026, 5, 21));
        dto.WeightKg.Should().Be(79.25m);
        dto.WaistCm.Should().Be(82.5m);
        dto.Note.Should().Be("Updated");
        dto.UpdatedAtUtc.Should().NotBeNull();

        var persisted = await Db.QueryAsync(context =>
            context.ClientBodyMetricEntries.SingleAsync());
        persisted.RecordedAt.Should().Be(new DateOnly(2026, 5, 21));
        persisted.WeightKg.Should().Be(79.25m);
    }

    [Fact]
    public async Task ClientCanDeleteOwnEntry()
    {
        var setup = await CreateClientWithMetricsAsync("body-metrics-delete");
        var entry = await CreateMetricAsync(
            setup.Metrics,
            new DateOnly(2026, 5, 20));

        var response = await setup.Metrics.DeleteAsync(entry.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var count = await Db.QueryAsync(context =>
            context.ClientBodyMetricEntries.CountAsync());
        count.Should().Be(0);
    }
}
