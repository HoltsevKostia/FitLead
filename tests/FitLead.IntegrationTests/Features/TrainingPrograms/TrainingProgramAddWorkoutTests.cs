using System.Net;
using FitLead.Application.Trainings.TrainingPrograms.Queries;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.TrainingPrograms;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainingProgramAddWorkoutTests(IntegrationTestFixture fixture)
    : TrainingProgramTestBase(fixture)
{
    [Fact]
    public async Task AddWorkout_WithOwnWorkoutAndValidSlot_ShouldAddProgramEntry()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-add-own");
        var client = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(client, weeksCount: 4, daysPerWeek: 7);
        var workoutId = await Workouts.CreateWorkoutAsync(trainer.Id, "Full Body A");

        var response = await client.AddWorkoutAsync(
            programId,
            workoutId,
            weekNumber: 2,
            dayNumber: 3);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var entries = await ReadProgramWorkoutsAsync(client, programId);
        var entry = entries.Should().ContainSingle().Subject;
        entry.WorkoutId.Should().Be(workoutId);
        entry.WorkoutName.Should().Be("Full Body A");
        entry.WeekNumber.Should().Be(2);
        entry.DayNumber.Should().Be(3);
        entry.OrderInDay.Should().Be(1);
    }

    [Fact]
    public async Task AddWorkout_WithAnotherTrainerWorkout_ShouldReturnForbidden()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-add-owner");
        var otherTrainer = await Users.RegisterTrainerAsync("tp-add-other");
        var client = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(client);
        var otherWorkoutId = await Workouts.CreateWorkoutAsync(otherTrainer.Id, "Other Trainer Workout");

        var response = await client.AddWorkoutAsync(programId, otherWorkoutId);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("workout.forbidden");
    }

    [Fact]
    public async Task AddWorkout_WithWeekOutsideRange_ShouldReturnValidationError()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-add-week-range");
        var client = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(client, weeksCount: 2, daysPerWeek: 7);
        var workoutId = await Workouts.CreateWorkoutAsync(trainer.Id);

        var response = await client.AddWorkoutAsync(programId, workoutId, weekNumber: 3, dayNumber: 1);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("training_program.workouts.week_number_out_of_range");
    }

    [Fact]
    public async Task AddWorkout_WithDayOutsideRange_ShouldReturnValidationError()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-add-day-range");
        var client = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(client, weeksCount: 4, daysPerWeek: 3);
        var workoutId = await Workouts.CreateWorkoutAsync(trainer.Id);

        var response = await client.AddWorkoutAsync(programId, workoutId, weekNumber: 1, dayNumber: 4);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("training_program.workouts.day_number_out_of_range");
    }
}
