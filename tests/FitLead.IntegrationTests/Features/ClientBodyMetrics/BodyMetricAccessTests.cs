using System.Net;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.ClientBodyMetrics;

public sealed class BodyMetricAccessTests(IntegrationTestFixture fixture)
    : BodyMetricsTestBase(fixture)
{
    [Fact]
    public async Task ClientCannotUpdateAnotherClientEntry()
    {
        var owner = await CreateClientWithMetricsAsync("body-metrics-owner-update");
        var other = await CreateClientWithMetricsAsync("body-metrics-other-update");
        var entry = await CreateMetricAsync(
            owner.Metrics,
            new DateOnly(2026, 5, 20),
            weightKg: 80);

        var response = await other.Metrics.UpdateAsync(
            entry.Id,
            new DateOnly(2026, 5, 21),
            weightKg: 70);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("body_metric_entry.not_found");
    }

    [Fact]
    public async Task ClientCannotDeleteAnotherClientEntry()
    {
        var owner = await CreateClientWithMetricsAsync("body-metrics-owner-delete");
        var other = await CreateClientWithMetricsAsync("body-metrics-other-delete");
        var entry = await CreateMetricAsync(
            owner.Metrics,
            new DateOnly(2026, 5, 20));

        var response = await other.Metrics.DeleteAsync(entry.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var count = await Db.QueryAsync(context =>
            context.ClientBodyMetricEntries.CountAsync());
        count.Should().Be(1);
    }

    [Fact]
    public async Task TrainerCannotMutateBodyMetricsEndpoint()
    {
        var trainer = await Users.RegisterTrainerAsync("body-metrics-trainer");
        var metrics = await Api.BodyMetricsAsync(trainer.Auth);

        var response = await metrics.CreateAsync(
            new DateOnly(2026, 5, 20),
            weightKg: 80);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
