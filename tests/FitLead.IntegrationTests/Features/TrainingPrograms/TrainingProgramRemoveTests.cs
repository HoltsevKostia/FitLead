using System.Net;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.TrainingPrograms;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainingProgramRemoveTests(IntegrationTestFixture fixture)
    : TrainingProgramTestBase(fixture)
{
    [Fact]
    public async Task RemoveWorkout_ShouldRemoveProgramEntry()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-remove");
        var client = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(client);
        var workoutId = await Workouts.CreateWorkoutAsync(trainer.Id);
        (await client.AddWorkoutAsync(programId, workoutId)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var entryId = (await ReadProgramWorkoutsAsync(client, programId)).Single().Id;

        var response = await client.RemoveWorkoutAsync(programId, entryId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var entries = await ReadProgramWorkoutsAsync(client, programId);
        entries.Should().BeEmpty();
    }
}
