using System.Net;
using FitLead.Application.Clients.ClientProfiles;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.TrainerClients;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainerClientProfileTests(IntegrationTestFixture fixture)
    : TrainerClientWorkspaceTestBase(fixture)
{
    [Fact]
    public async Task GetProfile_ForOwnClientWithNoProfile_ShouldReturnEmptyProfile()
    {
        var trainer = await Users.RegisterTrainerAsync("trainer-client-profile-empty-owner");
        var client = await Users.RegisterClientAsync("trainer-client-profile-empty-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerClients = await Api.TrainerClientsAsync(trainer.Auth);
        var response = await trainerClients.GetProfileAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.ReadRequiredJsonAsync<ClientProfileDto>();
        profile.ClientId.Should().Be(client.Id);
        profile.Goal.Should().BeNull();
        profile.CreatedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task GetProfile_ForOwnClient_ShouldReturnProfile()
    {
        var trainer = await Users.RegisterTrainerAsync("trainer-client-profile-owner");
        var client = await Users.RegisterClientAsync("trainer-client-profile-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);
        await CreateClientProfileAsync(client.Id);

        var trainerClients = await Api.TrainerClientsAsync(trainer.Auth);
        var response = await trainerClients.GetProfileAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.ReadRequiredJsonAsync<ClientProfileDto>();
        profile.ClientId.Should().Be(client.Id);
        profile.Goal.Should().Be("Build strength");
        profile.ExperienceLevel.Should().Be("Intermediate");
        profile.HeightCm.Should().Be(178);
        profile.Limitations.Should().Be("Knee pain after running");
    }

    [Fact]
    public async Task GetProfile_ForAnotherTrainerClient_ShouldReturnNotFound()
    {
        var ownerTrainer = await Users.RegisterTrainerAsync("trainer-client-profile-owner-trainer");
        var otherTrainer = await Users.RegisterTrainerAsync("trainer-client-profile-other-trainer");
        var client = await Users.RegisterClientAsync("trainer-client-profile-private-client");
        await CreateTrainerClientRelationshipAsync(ownerTrainer.Id, client.Id);

        var trainerClients = await Api.TrainerClientsAsync(otherTrainer.Auth);
        var response = await trainerClients.GetProfileAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProfile_AsClient_ShouldReturnForbidden()
    {
        var client = await Users.RegisterClientAsync("trainer-client-profile-forbidden-client");
        var trainerClients = await Api.TrainerClientsAsync(client.Auth);

        var response = await trainerClients.GetProfileAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
