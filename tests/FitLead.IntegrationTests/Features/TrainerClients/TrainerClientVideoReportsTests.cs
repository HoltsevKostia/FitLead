using System.Net;
using FitLead.Application.Users.Queries;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.TrainerClients;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainerClientVideoReportsTests(IntegrationTestFixture fixture)
    : TrainerClientWorkspaceTestBase(fixture)
{
    [Fact]
    public async Task GetVideoReports_ForOwnClient_ShouldReturnRecentReports()
    {
        var trainer = await Users.RegisterTrainerAsync("trainer-client-video-reports-owner");
        var client = await Users.RegisterClientAsync("trainer-client-video-reports-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);
        var reportId = await CreateVideoReportAsync(trainer.Id, client.Id);

        var trainerClients = await Api.TrainerClientsAsync(trainer.Auth);
        var response = await trainerClients.GetVideoReportsAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reports = await response.ReadRequiredJsonAsync<IReadOnlyList<TrainerClientVideoReportDto>>();
        reports.Should().ContainSingle();
        reports[0].ReportId.Should().Be(reportId);
        reports[0].Title.Should().Be("Squat check");
        reports[0].Description.Should().Be("Please review my squat");
        reports[0].Status.Should().Be("Submitted");
        reports[0].MediaCount.Should().Be(1);
    }

    [Fact]
    public async Task GetVideoReports_ForAnotherTrainerClient_ShouldReturnNotFound()
    {
        var ownerTrainer = await Users.RegisterTrainerAsync("trainer-client-video-reports-owner-trainer");
        var otherTrainer = await Users.RegisterTrainerAsync("trainer-client-video-reports-other-trainer");
        var client = await Users.RegisterClientAsync("trainer-client-video-reports-private-client");
        await CreateTrainerClientRelationshipAsync(ownerTrainer.Id, client.Id);

        var trainerClients = await Api.TrainerClientsAsync(otherTrainer.Auth);
        var response = await trainerClients.GetVideoReportsAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetVideoReports_AsClient_ShouldReturnForbidden()
    {
        var client = await Users.RegisterClientAsync("trainer-client-video-reports-forbidden-client");
        var trainerClients = await Api.TrainerClientsAsync(client.Auth);

        var response = await trainerClients.GetVideoReportsAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
