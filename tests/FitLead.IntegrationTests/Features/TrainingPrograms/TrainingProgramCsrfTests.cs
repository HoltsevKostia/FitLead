using System.Net;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.TrainingPrograms;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class TrainingProgramCsrfTests(IntegrationTestFixture fixture)
    : TrainingProgramTestBase(fixture)
{
    [Fact]
    public async Task MutatingEndpoints_WithoutCsrf_ShouldReturnBadRequest()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-csrf");
        var client = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(client);
        var workoutId = await Workouts.CreateWorkoutAsync(trainer.Id);
        (await client.AddWorkoutAsync(programId, workoutId)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var entryId = (await ReadProgramWorkoutsAsync(client, programId)).Single().Id;

        (await client.CreateAsync(includeCsrfHeader: false)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.AddWorkoutAsync(programId, workoutId, weekNumber: 1, dayNumber: 2, includeCsrfHeader: false)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.ReorderDayAsync(programId, 1, 1, [entryId], includeCsrfHeader: false)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.MoveWorkoutAsync(programId, entryId, 1, 1, 1, includeCsrfHeader: false)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.RemoveWorkoutAsync(programId, entryId, includeCsrfHeader: false)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.DeleteAsync(programId, includeCsrfHeader: false)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.ConfirmDeleteAsync(programId, "invalid-token", includeCsrfHeader: false)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetEndpoints_WithoutCsrf_ShouldRemainAllowed()
    {
        var trainer = await Users.RegisterTrainerAsync("tp-get-csrf");
        var client = await Api.TrainingProgramsAsync(trainer.Auth);
        var programId = await CreateProgramAsync(client);

        var listResponse = await client.GetAsync();
        var workoutsResponse = await client.GetWorkoutsAsync(programId);

        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        workoutsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
