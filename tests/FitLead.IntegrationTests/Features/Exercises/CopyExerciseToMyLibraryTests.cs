using System.Net;
using FitLead.Domain.Trainings.Exercises;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Exercises;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class CopyExerciseToMyLibraryTests : IntegrationTestBase
{
    private readonly TestUsers _users;
    private readonly TestExercises _exercises;
    private readonly TestApiClients _api;

    public CopyExerciseToMyLibraryTests(IntegrationTestFixture fixture) : base(fixture)
    {
        var db = new TestDb(fixture);
        _users = new TestUsers(fixture, db);
        _exercises = new TestExercises(db);
        _api = new TestApiClients(fixture);
    }

    [Fact]
    public async Task CopyToMyLibrary_WithPlatformExercise_ShouldCreateEditableTrainerCopy()
    {
        var trainer = await _users.RegisterTrainerAsync("copy-platform");
        var platformExerciseId = await _exercises.CreatePlatformExerciseAsync(
            muscleGroup: MuscleGroup.Chest,
            equipment: Equipment.Barbell);
        var client = await _api.ExercisesAsync(trainer.Auth);

        var copyResponse = await client.CopyToMyLibraryAsync(platformExerciseId);

        copyResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var copiedExerciseId = await copyResponse.ReadRequiredJsonAsync<Guid>();
        var copiedExercise = await _exercises.GetRequiredAsync(copiedExerciseId);

        copiedExercise.Source.Should().Be(ExerciseSource.Trainer);
        copiedExercise.OwnerTrainerId.Should().Be(trainer.Id);
        copiedExercise.CopiedFromExerciseId.Should().Be(platformExerciseId);
        copiedExercise.MuscleGroup.Should().Be(MuscleGroup.Chest);
        copiedExercise.Equipment.Should().Be(Equipment.Barbell);

        var updateResponse = await client.UpdateAsync(
            copiedExerciseId,
            name: "Моя адаптована вправа",
            description: "Опис адаптованої вправи");

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CopyToMyLibrary_WithOwnTrainerExercise_ShouldReturnBadRequest()
    {
        var trainer = await _users.RegisterTrainerAsync("copy-own");
        var exerciseId = await _exercises.CreateTrainerExerciseAsync(trainer.Id);
        var client = await _api.ExercisesAsync(trainer.Auth);

        var response = await client.CopyToMyLibraryAsync(exerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.copy.source_must_be_platform");
    }

    [Fact]
    public async Task CopyToMyLibrary_WithAnotherTrainerExercise_ShouldReturnNotFound()
    {
        var trainer = await _users.RegisterTrainerAsync("copy-other-current");
        var otherTrainer = await _users.RegisterTrainerAsync("copy-other-owner");
        var otherExerciseId = await _exercises.CreateTrainerExerciseAsync(otherTrainer.Id);
        var client = await _api.ExercisesAsync(trainer.Auth);

        var response = await client.CopyToMyLibraryAsync(otherExerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.not_found");
    }

    [Fact]
    public async Task CopyToMyLibrary_WhenCopyAlreadyExists_ShouldReturnConflict()
    {
        var trainer = await _users.RegisterTrainerAsync("copy-duplicate");
        var platformExerciseId = await _exercises.CreatePlatformExerciseAsync();
        var client = await _api.ExercisesAsync(trainer.Auth);

        var firstResponse = await client.CopyToMyLibraryAsync(platformExerciseId);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondResponse = await client.CopyToMyLibraryAsync(platformExerciseId);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await secondResponse.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.copy.already_exists");
    }
}
