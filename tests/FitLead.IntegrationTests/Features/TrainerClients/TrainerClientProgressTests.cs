using System.Net;
using FitLead.Application.Users.Queries;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.TrainerClients;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainerClientProgressTests(IntegrationTestFixture fixture)
    : TrainerClientWorkspaceTestBase(fixture)
{
    [Fact]
    public async Task GetProgress_ForOwnClient_ShouldReturnMetricsAndPhotos()
    {
        var trainer = await Users.RegisterTrainerAsync("trainer-client-progress-owner");
        var client = await Users.RegisterClientAsync("trainer-client-progress-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);
        var metricId = await CreateBodyMetricAsync(client.Id);
        var photoId = await CreateProgressPhotoAsync(client.Id);

        var trainerClients = await Api.TrainerClientsAsync(trainer.Auth);
        var response = await trainerClients.GetProgressAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var progress = await response.ReadRequiredJsonAsync<TrainerClientProgressDto>();
        progress.BodyMetrics.Should().ContainSingle();
        progress.BodyMetrics[0].Id.Should().Be(metricId);
        progress.BodyMetrics[0].WeightKg.Should().Be(78.5m);
        progress.BodyMetrics[0].WaistCm.Should().Be(84m);
        progress.ProgressPhotos.Should().ContainSingle();
        progress.ProgressPhotos[0].Id.Should().Be(photoId);
        progress.ProgressPhotos[0].Label.Should().Be("Front");
        progress.ProgressPhotos[0].MediaAsset.Kind.Should().Be("Image");
    }

    [Fact]
    public async Task GetProgress_ForAnotherTrainerClient_ShouldReturnNotFound()
    {
        var ownerTrainer = await Users.RegisterTrainerAsync("trainer-client-progress-owner-trainer");
        var otherTrainer = await Users.RegisterTrainerAsync("trainer-client-progress-other-trainer");
        var client = await Users.RegisterClientAsync("trainer-client-progress-private-client");
        await CreateTrainerClientRelationshipAsync(ownerTrainer.Id, client.Id);

        var trainerClients = await Api.TrainerClientsAsync(otherTrainer.Auth);
        var response = await trainerClients.GetProgressAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProgress_AsClient_ShouldReturnForbidden()
    {
        var client = await Users.RegisterClientAsync("trainer-client-progress-forbidden-client");
        var trainerClients = await Api.TrainerClientsAsync(client.Auth);

        var response = await trainerClients.GetProgressAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
