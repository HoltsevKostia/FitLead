using System.Net;
using FitLead.Application.Trainings.TrainingPrograms.Queries;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.TrainingPrograms;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainingProgramMoveTests(IntegrationTestFixture fixture)
    : TrainingProgramTestBase(fixture)
{
    [Fact]
    public async Task MoveWorkout_ToAnotherDay_ShouldMoveEntryAndReorderAffectedDays()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-move");
        var client = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(client);
        var firstWorkout = await Workouts.CreateWorkoutAsync(trainer.Id, "First");
        var secondWorkout = await Workouts.CreateWorkoutAsync(trainer.Id, "Second");

        (await client.AddWorkoutAsync(programId, firstWorkout, weekNumber: 1, dayNumber: 1)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.AddWorkoutAsync(programId, secondWorkout, weekNumber: 1, dayNumber: 1)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var secondEntryId = (await ReadProgramWorkoutsAsync(client, programId)).Single(x => x.WorkoutId == secondWorkout).Id;

        var response = await client.MoveWorkoutAsync(programId, secondEntryId, 2, 2, 1);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var afterMove = await ReadProgramWorkoutsAsync(client, programId);
        afterMove.Should().HaveCount(2);
        afterMove.Single(x => x.WorkoutId == firstWorkout).Should().Match<TrainingProgramWorkoutDto>(x =>
            x.WeekNumber == 1 && x.DayNumber == 1 && x.OrderInDay == 1);
        afterMove.Single(x => x.WorkoutId == secondWorkout).Should().Match<TrainingProgramWorkoutDto>(x =>
            x.WeekNumber == 2 && x.DayNumber == 2 && x.OrderInDay == 1);
    }

    [Fact]
    public async Task MoveWorkout_OutsideRange_ShouldReturnValidationError()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-move-range");
        var client = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(client, weeksCount: 2, daysPerWeek: 3);
        var workoutId = await Workouts.CreateWorkoutAsync(trainer.Id);
        (await client.AddWorkoutAsync(programId, workoutId)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var entryId = (await ReadProgramWorkoutsAsync(client, programId)).Single().Id;

        var response = await client.MoveWorkoutAsync(programId, entryId, 3, 1, 1);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("training_program.workouts.week_number_out_of_range");
    }

    [Fact]
    public async Task MoveWorkout_ToOccupiedOrder_ShouldKeepOrderInDayUnique()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-move-occupied");
        var client = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(client);
        var firstWorkout = await Workouts.CreateWorkoutAsync(trainer.Id, "First");
        var secondWorkout = await Workouts.CreateWorkoutAsync(trainer.Id, "Second");

        (await client.AddWorkoutAsync(programId, firstWorkout, weekNumber: 1, dayNumber: 1)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.AddWorkoutAsync(programId, secondWorkout, weekNumber: 1, dayNumber: 1)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var secondEntryId = (await ReadProgramWorkoutsAsync(client, programId)).Single(x => x.WorkoutId == secondWorkout).Id;

        var response = await client.MoveWorkoutAsync(programId, secondEntryId, 1, 1, 1);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var entries = await ReadProgramWorkoutsAsync(client, programId);
        entries.Select(x => x.OrderInDay).Should().OnlyHaveUniqueItems();
        entries.Select(x => x.WorkoutId).Should().Equal(secondWorkout, firstWorkout);
        entries.Select(x => x.OrderInDay).Should().Equal(1, 2);
    }
}
