using System.Net;
using FitLead.Application.Trainings.TrainingProgramAssignments.Commands;
using FitLead.Application.Trainings.TrainingPrograms.Queries;
using FitLead.Application.Trainings.WorkoutLogs.Commands;
using FitLead.Domain.Trainings.WorkoutLogs;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Features.TrainingPrograms;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class WorkoutLogUpsertTests(IntegrationTestFixture fixture)
    : TrainingProgramTestBase(fixture)
{
    [Fact]
    public async Task ClientCanCreateCompletedWorkoutLog()
    {
        var setup = await CreateAssignedProgramWorkoutAsync(
            "wl-completed-trainer",
            "wl-completed-client");
        var performedAtUtc = DateTime.UtcNow.AddHours(-2);

        var response = await setup.ClientPrograms.UpsertWorkoutLogAsync(
            setup.AssignmentId,
            setup.ProgramWorkoutId,
            nameof(WorkoutLogStatus.Completed),
            performedAtUtc,
            clientNote: "Hard finisher",
            difficultyRating: 8);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.ReadRequiredJsonAsync<WorkoutLogDto>();
        dto.Status.Should().Be(nameof(WorkoutLogStatus.Completed));
        dto.PerformedAtUtc.Should().BeCloseTo(performedAtUtc, TimeSpan.FromMilliseconds(1));
        dto.ClientNote.Should().Be("Hard finisher");
        dto.DifficultyRating.Should().Be(8);
        dto.UpdatedAtUtc.Should().BeNull();

        var persisted = await Db.QueryAsync(context =>
            context.WorkoutLogs.SingleAsync());
        persisted.Id.Should().Be(dto.Id);
        persisted.AssignedTrainingProgramId.Should().Be(setup.AssignmentId);
        persisted.TrainingProgramWorkoutId.Should().Be(setup.ProgramWorkoutId);
        persisted.ClientId.Should().Be(setup.ClientId);
        persisted.TrainerId.Should().Be(setup.TrainerId);
        persisted.Status.Should().Be(WorkoutLogStatus.Completed);
    }

    [Fact]
    public async Task CompletedWithoutPerformedAt_ShouldDefaultPerformedAtToNow()
    {
        var setup = await CreateAssignedProgramWorkoutAsync(
            "wl-completed-now-trainer",
            "wl-completed-now-client");
        var beforeRequestUtc = DateTime.UtcNow;

        var response = await setup.ClientPrograms.UpsertWorkoutLogAsync(
            setup.AssignmentId,
            setup.ProgramWorkoutId,
            nameof(WorkoutLogStatus.Completed));

        var afterRequestUtc = DateTime.UtcNow;
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.ReadRequiredJsonAsync<WorkoutLogDto>();
        dto.PerformedAtUtc.Should().NotBeNull();
        dto.PerformedAtUtc!.Value.Should().BeOnOrAfter(beforeRequestUtc.AddSeconds(-1));
        dto.PerformedAtUtc!.Value.Should().BeOnOrBefore(afterRequestUtc.AddSeconds(1));
    }

    [Fact]
    public async Task ClientCanCreateSkippedWorkoutLog()
    {
        var setup = await CreateAssignedProgramWorkoutAsync(
            "wl-skipped-trainer",
            "wl-skipped-client");

        var response = await setup.ClientPrograms.UpsertWorkoutLogAsync(
            setup.AssignmentId,
            setup.ProgramWorkoutId,
            nameof(WorkoutLogStatus.Skipped),
            clientNote: "Felt sick");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.ReadRequiredJsonAsync<WorkoutLogDto>();
        dto.Status.Should().Be(nameof(WorkoutLogStatus.Skipped));
        dto.PerformedAtUtc.Should().BeNull();
        dto.DifficultyRating.Should().BeNull();
        dto.ClientNote.Should().Be("Felt sick");
    }

    [Fact]
    public async Task UpsertExistingLog_ShouldUpdateSingleRow()
    {
        var setup = await CreateAssignedProgramWorkoutAsync(
            "wl-update-trainer",
            "wl-update-client");
        (await setup.ClientPrograms.UpsertWorkoutLogAsync(
            setup.AssignmentId,
            setup.ProgramWorkoutId,
            nameof(WorkoutLogStatus.Completed),
            DateTime.UtcNow.AddDays(-1),
            difficultyRating: 6))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await setup.ClientPrograms.UpsertWorkoutLogAsync(
            setup.AssignmentId,
            setup.ProgramWorkoutId,
            nameof(WorkoutLogStatus.Skipped),
            clientNote: "Rest day");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.ReadRequiredJsonAsync<WorkoutLogDto>();
        dto.Status.Should().Be(nameof(WorkoutLogStatus.Skipped));
        dto.PerformedAtUtc.Should().BeNull();
        dto.DifficultyRating.Should().BeNull();
        dto.ClientNote.Should().Be("Rest day");
        dto.UpdatedAtUtc.Should().NotBeNull();

        var logs = await Db.QueryAsync(context => context.WorkoutLogs.ToListAsync());
        logs.Should().ContainSingle();
        logs.Single().Status.Should().Be(WorkoutLogStatus.Skipped);
    }

    [Fact]
    public async Task SkippedWithCompletedOnlyFields_ShouldReturnValidationError()
    {
        var setup = await CreateAssignedProgramWorkoutAsync(
            "wl-skipped-invalid-trainer",
            "wl-skipped-invalid-client");

        var response = await setup.ClientPrograms.UpsertWorkoutLogAsync(
            setup.AssignmentId,
            setup.ProgramWorkoutId,
            nameof(WorkoutLogStatus.Skipped),
            DateTime.UtcNow,
            difficultyRating: 8);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("workout_log.skipped_performed_at_not_allowed");
    }

    [Fact]
    public async Task OtherClientCannotCreateLogForAssignment()
    {
        var setup = await CreateAssignedProgramWorkoutAsync(
            "wl-other-trainer",
            "wl-owner-client");
        var otherClient = await Users.RegisterClientAsync("wl-other-client");
        var otherClientPrograms = await Api.ClientTrainingProgramsAsync(otherClient.Auth);

        var response = await otherClientPrograms.UpsertWorkoutLogAsync(
            setup.AssignmentId,
            setup.ProgramWorkoutId,
            nameof(WorkoutLogStatus.Completed));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("workout_log.assignment_workout_not_found");
    }

    [Fact]
    public async Task TrainerCannotCreateWorkoutLog()
    {
        var setup = await CreateAssignedProgramWorkoutAsync(
            "wl-trainer-forbidden",
            "wl-trainer-client");
        var trainerClientPrograms = await Api.ClientTrainingProgramsAsync(setup.TrainerAuth);

        var response = await trainerClientPrograms.UpsertWorkoutLogAsync(
            setup.AssignmentId,
            setup.ProgramWorkoutId,
            nameof(WorkoutLogStatus.Completed));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpsertWithoutCsrf_ShouldReturnBadRequest()
    {
        var setup = await CreateAssignedProgramWorkoutAsync(
            "wl-csrf-trainer",
            "wl-csrf-client");

        var response = await setup.ClientPrograms.UpsertWorkoutLogAsync(
            setup.AssignmentId,
            setup.ProgramWorkoutId,
            nameof(WorkoutLogStatus.Completed),
            includeCsrfHeader: false);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<AssignedProgramWorkoutSetup> CreateAssignedProgramWorkoutAsync(
        string trainerPrefix,
        string clientPrefix)
    {
        var trainer = await Users.RegisterTrainerAsync(trainerPrefix);
        var client = await Users.RegisterClientAsync(clientPrefix);
        await CreateTrainerClientRelationshipAsync(trainer.Id, client.Id);

        var trainerPrograms = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(trainerPrograms);
        var workoutId = await Workouts.CreateWorkoutAsync(trainer.Id);
        (await trainerPrograms.AddWorkoutAsync(programId, workoutId))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        var programWorkout = (await ReadProgramWorkoutsAsync(trainerPrograms, programId))
            .Should()
            .ContainSingle()
            .Subject;
        var assignmentResponse = await trainerPrograms.AssignToClientAsync(programId, client.Id);
        assignmentResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var assignment = await assignmentResponse.ReadRequiredJsonAsync<AssignTrainingProgramToClientResult>();
        var clientPrograms = await Api.ClientTrainingProgramsAsync(client.Auth);

        return new AssignedProgramWorkoutSetup(
            trainer.Auth,
            trainer.Id,
            client.Id,
            assignment.AssignmentId,
            programWorkout.Id,
            clientPrograms);
    }

    private sealed record AssignedProgramWorkoutSetup(
        AuthTestClient TrainerAuth,
        Guid TrainerId,
        Guid ClientId,
        Guid AssignmentId,
        Guid ProgramWorkoutId,
        ClientTrainingProgramsTestClient ClientPrograms);
}
