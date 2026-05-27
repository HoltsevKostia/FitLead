using System.Net;
using FitLead.Application.Users.Queries;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.TrainerClients;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainerClientWorkoutLogsTests(IntegrationTestFixture fixture)
    : TrainerClientWorkspaceTestBase(fixture)
{
    [Fact]
    public async Task GetWorkoutLogs_ForOwnClient_ShouldReturnRecentLogs()
    {
        var trainer = await Users.RegisterTrainerAsync("trainer-client-workout-logs-owner");
        var client = await Users.RegisterClientAsync("trainer-client-workout-logs-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);
        var assignment = await CreateAssignedWorkoutAsync(trainer.Id, client.Id);
        var logId = await CreateCompletedWorkoutLogAsync(
            trainer.Id,
            client.Id,
            assignment.AssignmentId,
            assignment.ProgramWorkoutId);

        var trainerClients = await Api.TrainerClientsAsync(trainer.Auth);
        var response = await trainerClients.GetWorkoutLogsAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var logs = await response.ReadRequiredJsonAsync<IReadOnlyList<TrainerClientWorkoutLogDto>>();
        logs.Should().ContainSingle();
        logs[0].LogId.Should().Be(logId);
        logs[0].AssignmentId.Should().Be(assignment.AssignmentId);
        logs[0].ProgramId.Should().Be(assignment.ProgramId);
        logs[0].ProgramWorkoutId.Should().Be(assignment.ProgramWorkoutId);
        logs[0].WorkoutId.Should().Be(assignment.WorkoutId);
        logs[0].Status.Should().Be("Completed");
        logs[0].DifficultyRating.Should().Be(8);
        logs[0].ClientNote.Should().Be("Felt strong today");
    }

    [Fact]
    public async Task GetWorkoutLogs_ForAnotherTrainerClient_ShouldReturnNotFound()
    {
        var ownerTrainer = await Users.RegisterTrainerAsync("trainer-client-workout-logs-owner-trainer");
        var otherTrainer = await Users.RegisterTrainerAsync("trainer-client-workout-logs-other-trainer");
        var client = await Users.RegisterClientAsync("trainer-client-workout-logs-private-client");
        await CreateTrainerClientRelationshipAsync(ownerTrainer.Id, client.Id);

        var trainerClients = await Api.TrainerClientsAsync(otherTrainer.Auth);
        var response = await trainerClients.GetWorkoutLogsAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetWorkoutLogs_AsClient_ShouldReturnForbidden()
    {
        var client = await Users.RegisterClientAsync("trainer-client-workout-logs-forbidden-client");
        var trainerClients = await Api.TrainerClientsAsync(client.Auth);

        var response = await trainerClients.GetWorkoutLogsAsync(client.Id);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
