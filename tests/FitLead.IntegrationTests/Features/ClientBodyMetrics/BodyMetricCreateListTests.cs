using System.Net;
using FitLead.Application.Clients.BodyMetrics;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.ClientBodyMetrics;

public sealed class BodyMetricCreateListTests(IntegrationTestFixture fixture)
    : BodyMetricsTestBase(fixture)
{
    [Fact]
    public async Task ClientCanCreateMetricEntry()
    {
        var setup = await CreateClientWithMetricsAsync("body-metrics-create");
        var recordedAt = new DateOnly(2026, 5, 25);

        var response = await setup.Metrics.CreateAsync(
            recordedAt,
            weightKg: 78.5m,
            bodyFatPercent: 18.2m,
            chestCm: 101.4m,
            waistCm: 83.5m,
            hipsCm: 96.1m,
            armCm: 34.2m,
            thighCm: 57.3m,
            note: "First check-in");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var dto = await response.ReadRequiredJsonAsync<ClientBodyMetricEntryDto>();
        dto.ClientId.Should().Be(setup.ClientId);
        dto.RecordedAt.Should().Be(recordedAt);
        dto.WeightKg.Should().Be(78.5m);
        dto.BodyFatPercent.Should().Be(18.2m);
        dto.ChestCm.Should().Be(101.4m);
        dto.WaistCm.Should().Be(83.5m);
        dto.HipsCm.Should().Be(96.1m);
        dto.ArmCm.Should().Be(34.2m);
        dto.ThighCm.Should().Be(57.3m);
        dto.Note.Should().Be("First check-in");
        dto.UpdatedAtUtc.Should().BeNull();

        var persisted = await Db.QueryAsync(context =>
            context.ClientBodyMetricEntries.SingleAsync());
        persisted.Id.Should().Be(dto.Id);
        persisted.ClientId.Should().Be(setup.ClientId);
    }

    [Fact]
    public async Task ClientCanListOwnEntries()
    {
        var own = await CreateClientWithMetricsAsync("body-metrics-list-own");
        var other = await CreateClientWithMetricsAsync("body-metrics-list-other");
        var older = await CreateMetricAsync(
            own.Metrics,
            new DateOnly(2026, 5, 20),
            weightKg: 80);
        var newer = await CreateMetricAsync(
            own.Metrics,
            new DateOnly(2026, 5, 25),
            weightKg: 79);
        await CreateMetricAsync(
            other.Metrics,
            new DateOnly(2026, 5, 24),
            weightKg: 70);

        var response = await own.Metrics.GetAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var entries = await response.ReadRequiredJsonAsync<IReadOnlyList<ClientBodyMetricEntryDto>>();
        entries.Select(entry => entry.Id)
            .Should()
            .Equal(newer.Id, older.Id);
        entries.Should().OnlyContain(entry => entry.ClientId == own.ClientId);
    }
}
