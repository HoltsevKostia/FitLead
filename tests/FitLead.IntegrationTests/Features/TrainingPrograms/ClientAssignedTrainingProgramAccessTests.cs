using System.Net;
using FitLead.Application.Trainings.TrainingProgramAssignments.Commands;
using FitLead.Application.Trainings.TrainingProgramAssignments.Queries;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.TrainingPrograms;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class ClientAssignedTrainingProgramAccessTests(IntegrationTestFixture fixture)
    : TrainingProgramTestBase(fixture)
{
    [Fact]
    public async Task ClientAssignedPrograms_ShouldReturnOnlyAccessibleAssignments()
    {
        var trainer = await Users.RegisterTrainerAsync("client-programs-trainer");
        var client = await Users.RegisterClientAsync("client-programs-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var clientPrograms = await Api.ClientTrainingProgramsAsync(client.Auth);
        var activeProgramId = await CreateProgramAsync(trainerPrograms, "Active Program");
        var revokedProgramId = await CreateProgramAsync(trainerPrograms, "Revoked Program");
        var expiredProgramId = await CreateProgramAsync(trainerPrograms, "Expired Program");
        var unassignedProgramId = await CreateProgramAsync(trainerPrograms, "Unassigned Program");

        var activeResponse = await trainerPrograms.AssignToClientAsync(activeProgramId, client.Id);
        var active = await activeResponse.ReadRequiredJsonAsync<AssignTrainingProgramToClientResult>();

        var revokedResponse = await trainerPrograms.AssignToClientAsync(revokedProgramId, client.Id);
        var revoked = await revokedResponse.ReadRequiredJsonAsync<AssignTrainingProgramToClientResult>();
        (await trainerPrograms.RevokeAssignmentAsync(revokedProgramId, revoked.AssignmentId))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        await AddExpiredActiveAssignmentAsync(
            trainer.Id,
            client.Id,
            expiredProgramId,
            assignedAtUtc: DateTime.UtcNow.AddDays(-4),
            expiresAtUtc: DateTime.UtcNow.AddDays(-2));

        var response = await clientPrograms.GetAssignedProgramsAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var programs = await response.ReadRequiredJsonAsync<IReadOnlyList<ClientAssignedTrainingProgramDto>>();
        var program = programs.Should().ContainSingle().Subject;
        program.AssignmentId.Should().Be(active.AssignmentId);
        program.ProgramId.Should().Be(activeProgramId);
        program.Title.Should().Be("Active Program");
        program.TrainerId.Should().Be(trainer.Id);
        program.TrainerName.Should().Be("Test Trainer");
        programs.Should().NotContain(x => x.ProgramId == revokedProgramId);
        programs.Should().NotContain(x => x.ProgramId == expiredProgramId);
        programs.Should().NotContain(x => x.ProgramId == unassignedProgramId);
    }

    [Fact]
    public async Task ClientAssignedProgramDetails_ShouldReturnAccessibleProgramWorkouts()
    {
        var trainer = await Users.RegisterTrainerAsync("client-program-details-trainer");
        var client = await Users.RegisterClientAsync("client-program-details-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var clientPrograms = await Api.ClientTrainingProgramsAsync(client.Auth);
        var programId = await CreateProgramAsync(trainerPrograms, "Details Program", weeksCount: 2, daysPerWeek: 3);
        var workoutId = await Workouts.CreateWorkoutAsync(trainer.Id, "Details Workout");
        (await trainerPrograms.AddWorkoutAsync(programId, workoutId, weekNumber: 2, dayNumber: 3))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        var assignResponse = await trainerPrograms.AssignToClientAsync(programId, client.Id);
        var assignment = await assignResponse.ReadRequiredJsonAsync<AssignTrainingProgramToClientResult>();

        var response = await clientPrograms.GetAssignedProgramDetailsAsync(assignment.AssignmentId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var details = await response.ReadRequiredJsonAsync<ClientAssignedTrainingProgramDetailsDto>();
        details.AssignmentId.Should().Be(assignment.AssignmentId);
        details.ProgramId.Should().Be(programId);
        details.Title.Should().Be("Details Program");
        details.WeeksCount.Should().Be(2);
        details.DaysPerWeek.Should().Be(3);
        var workout = details.Workouts.Should().ContainSingle().Subject;
        workout.WorkoutId.Should().Be(workoutId);
        workout.WeekNumber.Should().Be(2);
        workout.DayNumber.Should().Be(3);
        workout.OrderInDay.Should().Be(1);
    }

    [Fact]
    public async Task ClientCannotReadOtherClientAssignmentDetails()
    {
        var trainer = await Users.RegisterTrainerAsync("client-program-other-trainer");
        var ownerClient = await Users.RegisterClientAsync("client-program-owner-client");
        var otherClient = await Users.RegisterClientAsync("client-program-other-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, ownerClient.Id);
        await CreateTrainerClientRelationshipAsync(trainer.Id, otherClient.Id);

        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var otherClientPrograms = await Api.ClientTrainingProgramsAsync(otherClient.Auth);
        var programId = await CreateProgramAsync(trainerPrograms);
        var assignResponse = await trainerPrograms.AssignToClientAsync(programId, ownerClient.Id);
        var assignment = await assignResponse.ReadRequiredJsonAsync<AssignTrainingProgramToClientResult>();

        var response = await otherClientPrograms.GetAssignedProgramDetailsAsync(assignment.AssignmentId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("training_program.assignment.not_found");
    }

    [Fact]
    public async Task ClientCannotSeeUnassignedProgramDirectly()
    {
        var trainer = await Users.RegisterTrainerAsync("client-template-trainer");
        var client = await Users.RegisterClientAsync("client-template-client");
        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var clientTrainerPrograms = await Api.TrainingProgramsAsync(client.Auth);
        var programId = await CreateProgramAsync(trainerPrograms);

        var response = await clientTrainerPrograms.GetByIdAsync(programId);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RevokedAssignmentDetails_ShouldReturnNotFoundForClient()
    {
        var trainer = await Users.RegisterTrainerAsync("client-revoked-details-trainer");
        var client = await Users.RegisterClientAsync("client-revoked-details-client");
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var clientPrograms = await Api.ClientTrainingProgramsAsync(client.Auth);
        var programId = await CreateProgramAsync(trainerPrograms);
        var assignResponse = await trainerPrograms.AssignToClientAsync(programId, client.Id);
        var assignment = await assignResponse.ReadRequiredJsonAsync<AssignTrainingProgramToClientResult>();
        (await trainerPrograms.RevokeAssignmentAsync(programId, assignment.AssignmentId))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await clientPrograms.GetAssignedProgramDetailsAsync(assignment.AssignmentId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("training_program.assignment.not_found");
    }

    [Fact]
    public async Task TrainerCannotUseClientAssignedProgramsEndpoint()
    {
        var trainer = await Users.RegisterTrainerAsync("client-endpoint-trainer");
        var trainerClientPrograms = await Api.ClientTrainingProgramsAsync(trainer.Auth);

        var response = await trainerClientPrograms.GetAssignedProgramsAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task AddExpiredActiveAssignmentAsync(
        Guid trainerId,
        Guid clientId,
        Guid programId,
        DateTime assignedAtUtc,
        DateTime expiresAtUtc)
    {
        var assignment = AssignedTrainingProgram.AssignManually(
            trainerId,
            clientId,
            programId,
            assignedAtUtc,
            expiresAtUtc);
        assignment.IsSuccess.Should().BeTrue();

        await Db.ExecuteAsync(async context =>
        {
            await context.AssignedTrainingPrograms.AddAsync(assignment.Value);
            await context.SaveChangesAsync();
        });
    }
}
