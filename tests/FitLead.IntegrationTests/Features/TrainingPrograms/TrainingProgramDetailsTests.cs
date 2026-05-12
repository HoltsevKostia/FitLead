using System.Net;
using FitLead.Application.Trainings.TrainingPrograms.Queries;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.TrainingPrograms;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainingProgramDetailsTests(IntegrationTestFixture fixture)
    : TrainingProgramTestBase(fixture)
{
    [Fact]
    public async Task Details_ShouldReturnProgramShapeAndWorkoutsSortedByWeekDayOrder()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-details");
        var client = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(client, title: "Details Program", weeksCount: 4, daysPerWeek: 7);
        var workoutA = await Workouts.CreateWorkoutAsync(trainer.Id, "Full Body A");
        var workoutB = await Workouts.CreateWorkoutAsync(trainer.Id, "Full Body B");
        var workoutC = await Workouts.CreateWorkoutAsync(trainer.Id, "Full Body C");

        (await client.AddWorkoutAsync(programId, workoutC, weekNumber: 2, dayNumber: 1)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.AddWorkoutAsync(programId, workoutB, weekNumber: 1, dayNumber: 3)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.AddWorkoutAsync(programId, workoutA, weekNumber: 1, dayNumber: 1)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var programsResponse = await client.GetAsync();
        var programs = await programsResponse.ReadRequiredJsonAsync<IReadOnlyList<TrainingProgramDto>>();
        var program = programs.Should().ContainSingle().Subject;
        program.Id.Should().Be(programId);
        program.WeeksCount.Should().Be(4);
        program.DaysPerWeek.Should().Be(7);

        var entries = await ReadProgramWorkoutsAsync(client, programId);
        entries.Select(x => x.WorkoutId).Should().Equal(workoutA, workoutB, workoutC);
        entries.Select(x => (x.WeekNumber, x.DayNumber, x.OrderInDay))
            .Should()
            .Equal((1, 1, 1), (1, 3, 1), (2, 1, 1));
    }
}
