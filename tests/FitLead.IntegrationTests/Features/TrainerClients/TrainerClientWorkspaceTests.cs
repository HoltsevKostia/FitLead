using System.Net;
using FitLead.Application.Users.Queries;
using FitLead.Infrastructure.Persistence.Models;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.TrainerClients;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainerClientWorkspaceTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private readonly TestDb _db = new(fixture);
    private readonly TestUsers _users = new(fixture, new TestDb(fixture));
    private readonly TestApiClients _api = new(fixture);

    [Fact]
    public async Task GetWorkspace_ForOwnClient_ShouldReturnHeaderData()
    {
        var trainer = await _users.RegisterTrainerAsync("trainer-client-workspace-owner");
        var client = await _users.RegisterClientAsync("trainer-client-workspace-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerClients = await _api.TrainerClientsAsync(trainer.Auth);
        var response = await trainerClients.GetWorkspaceAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var workspace = await response.ReadRequiredJsonAsync<TrainerClientWorkspaceDto>();
        workspace.ClientId.Should().Be(client.Id);
        workspace.Email.Should().Be(client.Email);
        workspace.FullName.Should().Be("Test Client");
    }

    [Fact]
    public async Task GetWorkspace_ForAnotherTrainerClient_ShouldReturnNotFound()
    {
        var ownerTrainer = await _users.RegisterTrainerAsync("trainer-client-workspace-owner-trainer");
        var otherTrainer = await _users.RegisterTrainerAsync("trainer-client-workspace-other-trainer");
        var client = await _users.RegisterClientAsync("trainer-client-workspace-private-client");
        await CreateTrainerClientRelationshipAsync(ownerTrainer.Id, client.Id);

        var trainerClients = await _api.TrainerClientsAsync(otherTrainer.Auth);
        var response = await trainerClients.GetWorkspaceAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetWorkspace_AsClient_ShouldReturnForbidden()
    {
        var client = await _users.RegisterClientAsync("trainer-client-workspace-forbidden-client");
        var trainerClients = await _api.TrainerClientsAsync(client.Auth);

        var response = await trainerClients.GetWorkspaceAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task CreateTrainerClientRelationshipAsync(Guid trainerId, Guid clientId)
    {
        await _db.ExecuteAsync(async context =>
        {
            await context.TrainerClients.AddAsync(new TrainerClient(trainerId, clientId));
            await context.SaveChangesAsync();
        });
    }
}
