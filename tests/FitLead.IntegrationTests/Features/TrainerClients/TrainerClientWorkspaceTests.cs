using System.Net;
using FitLead.Application.Users.Queries;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using FitLead.Domain.Trainings.TrainingPrograms;
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

    [Fact]
    public async Task GetOverviewSummary_ForOwnClient_ShouldReturnSummary()
    {
        var trainer = await _users.RegisterTrainerAsync("trainer-client-overview-owner");
        var client = await _users.RegisterClientAsync("trainer-client-overview-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerClients = await _api.TrainerClientsAsync(trainer.Auth);
        var response = await trainerClients.GetOverviewSummaryAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var overview = await response.ReadRequiredJsonAsync<TrainerClientOverviewSummaryDto>();
        overview.ActiveProgram.Should().BeNull();
        overview.WorkoutLogCounts.Completed.Should().Be(0);
        overview.WorkoutLogCounts.Skipped.Should().Be(0);
        overview.WorkoutLogCounts.Pending.Should().Be(0);
    }

    [Fact]
    public async Task GetOverviewSummary_ForAnotherTrainerClient_ShouldReturnNotFound()
    {
        var ownerTrainer = await _users.RegisterTrainerAsync("trainer-client-overview-owner-trainer");
        var otherTrainer = await _users.RegisterTrainerAsync("trainer-client-overview-other-trainer");
        var client = await _users.RegisterClientAsync("trainer-client-overview-private-client");
        await CreateTrainerClientRelationshipAsync(ownerTrainer.Id, client.Id);

        var trainerClients = await _api.TrainerClientsAsync(otherTrainer.Auth);
        var response = await trainerClients.GetOverviewSummaryAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPrograms_ForOwnClient_ShouldReturnAssignedPrograms()
    {
        var trainer = await _users.RegisterTrainerAsync("trainer-client-programs-owner");
        var client = await _users.RegisterClientAsync("trainer-client-programs-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);
        var program = await CreateAssignedProgramAsync(trainer.Id, client.Id, "Strength Base");

        var trainerClients = await _api.TrainerClientsAsync(trainer.Auth);
        var response = await trainerClients.GetProgramsAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var programs = await response.ReadRequiredJsonAsync<IReadOnlyList<TrainerClientProgramDto>>();
        programs.Should().ContainSingle();
        programs[0].ProgramId.Should().Be(program.ProgramId);
        programs[0].AssignmentId.Should().Be(program.AssignmentId);
        programs[0].ProgramTitle.Should().Be("Strength Base");
        programs[0].Status.Should().Be(nameof(AssignedProgramStatus.Active));
        programs[0].WorkoutLogCounts.Completed.Should().Be(0);
        programs[0].WorkoutLogCounts.Skipped.Should().Be(0);
        programs[0].WorkoutLogCounts.Pending.Should().Be(0);
    }

    [Fact]
    public async Task GetPrograms_ForAnotherTrainerClient_ShouldReturnNotFound()
    {
        var ownerTrainer = await _users.RegisterTrainerAsync("trainer-client-programs-owner-trainer");
        var otherTrainer = await _users.RegisterTrainerAsync("trainer-client-programs-other-trainer");
        var client = await _users.RegisterClientAsync("trainer-client-programs-private-client");
        await CreateTrainerClientRelationshipAsync(ownerTrainer.Id, client.Id);
        await CreateAssignedProgramAsync(ownerTrainer.Id, client.Id, "Private Program");

        var trainerClients = await _api.TrainerClientsAsync(otherTrainer.Auth);
        var response = await trainerClients.GetProgramsAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPrograms_AsClient_ShouldReturnForbidden()
    {
        var client = await _users.RegisterClientAsync("trainer-client-programs-forbidden-client");
        var trainerClients = await _api.TrainerClientsAsync(client.Auth);

        var response = await trainerClients.GetProgramsAsync(client.Id);

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

    private async Task<(Guid ProgramId, Guid AssignmentId)> CreateAssignedProgramAsync(
        Guid trainerId,
        Guid clientId,
        string title)
    {
        var programResult = TrainingProgram.Create(trainerId, title, weeksCount: 4, daysPerWeek: 3);
        programResult.IsSuccess.Should().BeTrue();
        var assignmentResult = AssignedTrainingProgram.AssignManually(
            trainerId,
            clientId,
            programResult.Value.Id,
            DateTime.UtcNow);
        assignmentResult.IsSuccess.Should().BeTrue();

        await _db.ExecuteAsync(async context =>
        {
            await context.TrainingPrograms.AddAsync(programResult.Value);
            await context.AssignedTrainingPrograms.AddAsync(assignmentResult.Value);
            await context.SaveChangesAsync();
        });

        return (programResult.Value.Id, assignmentResult.Value.Id);
    }
}
