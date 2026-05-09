using System.Net;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Exercises;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class ExerciseMutationRulesTests : IntegrationTestBase
{
    private readonly TestUsers _users;
    private readonly TestExercises _exercises;
    private readonly TestWorkouts _workouts;
    private readonly TestApiClients _api;

    public ExerciseMutationRulesTests(IntegrationTestFixture fixture) : base(fixture)
    {
        var db = new TestDb(fixture);
        _users = new TestUsers(fixture, db);
        _exercises = new TestExercises(db);
        _workouts = new TestWorkouts(db);
        _api = new TestApiClients(fixture);
    }

    [Fact]
    public async Task Update_WithOwnExercise_ShouldUpdateExercise()
    {
        var trainer = await _users.RegisterTrainerAsync("exercise-update-own");
        var exerciseId = await _exercises.CreateTrainerExerciseAsync(trainer.Id, "Власна вправа");
        var client = await _api.ExercisesAsync(trainer.Auth);

        var response = await client.UpdateAsync(
            exerciseId,
            name: "Оновлена власна вправа",
            description: "Новий опис");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var exercise = await _exercises.GetRequiredAsync(exerciseId);
        exercise.Name.Should().Be("Оновлена власна вправа");
        exercise.Description.Should().Be("Новий опис");
    }

    [Fact]
    public async Task Update_WithPlatformExercise_ShouldReturnNotFound()
    {
        var trainer = await _users.RegisterTrainerAsync("exercise-update-platform");
        var platformExerciseId = await _exercises.CreatePlatformExerciseAsync("Платформна вправа");
        var client = await _api.ExercisesAsync(trainer.Auth);

        var response = await client.UpdateAsync(platformExerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.not_found");
    }

    [Fact]
    public async Task Update_WithAnotherTrainerExercise_ShouldReturnNotFound()
    {
        var trainer = await _users.RegisterTrainerAsync("exercise-update-trainer");
        var otherTrainer = await _users.RegisterTrainerAsync("exercise-update-other");
        var otherExerciseId = await _exercises.CreateTrainerExerciseAsync(otherTrainer.Id, "Чужа вправа");
        var client = await _api.ExercisesAsync(trainer.Auth);

        var response = await client.UpdateAsync(otherExerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.not_found");
    }

    [Fact]
    public async Task Delete_WithOwnUnusedExercise_ShouldDeleteExercise()
    {
        var trainer = await _users.RegisterTrainerAsync("exercise-delete-own");
        var exerciseId = await _exercises.CreateTrainerExerciseAsync(trainer.Id, "Вправа для видалення");
        var client = await _api.ExercisesAsync(trainer.Auth);

        var response = await client.DeleteAsync(exerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _exercises.ExistsAsync(exerciseId)).Should().BeFalse();
    }

    [Fact]
    public async Task Delete_WithPlatformExercise_ShouldReturnNotFound()
    {
        var trainer = await _users.RegisterTrainerAsync("exercise-delete-platform");
        var platformExerciseId = await _exercises.CreatePlatformExerciseAsync("Платформна вправа для delete");
        var client = await _api.ExercisesAsync(trainer.Auth);

        var response = await client.DeleteAsync(platformExerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.not_found");
    }

    [Fact]
    public async Task Delete_WithAnotherTrainerExercise_ShouldReturnNotFound()
    {
        var trainer = await _users.RegisterTrainerAsync("exercise-delete-trainer");
        var otherTrainer = await _users.RegisterTrainerAsync("exercise-delete-other");
        var otherExerciseId = await _exercises.CreateTrainerExerciseAsync(otherTrainer.Id, "Чужа вправа для delete");
        var client = await _api.ExercisesAsync(trainer.Auth);

        var response = await client.DeleteAsync(otherExerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.not_found");
    }

    [Fact]
    public async Task Delete_WithUsedOwnExercise_ShouldRequireConfirmationAndConfirmDelete()
    {
        var trainer = await _users.RegisterTrainerAsync("exercise-delete-used");
        var exerciseId = await _exercises.CreateUsedTrainerExerciseAsync(trainer.Id);
        var client = await _api.ExercisesAsync(trainer.Auth);

        var dryRunResponse = await client.DeleteAsync(exerciseId);

        dryRunResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var (errorCode, token) = await ReadDeletionConflictAsync(dryRunResponse);
        errorCode.Should().Be("exercise.in_use");
        token.Should().NotBeNullOrWhiteSpace();

        var confirmResponse = await client.ConfirmDeleteAsync(exerciseId, token);

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _exercises.ExistsAsync(exerciseId)).Should().BeFalse();
        (await _workouts.HasAnyExerciseLinkAsync(exerciseId)).Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmDelete_WithPlatformExercise_ShouldReturnNotFound()
    {
        var trainer = await _users.RegisterTrainerAsync("exercise-confirm-platform");
        var platformExerciseId = await _exercises.CreatePlatformExerciseAsync("Платформна вправа для confirm");
        var client = await _api.ExercisesAsync(trainer.Auth);

        var response = await client.ConfirmDeleteAsync(platformExerciseId, "invalid-token");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.not_found");
    }

    private static async Task<(string? ErrorCode, string ConfirmationToken)> ReadDeletionConflictAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var json = await System.Text.Json.JsonDocument.ParseAsync(stream);
        var errorCode = json.RootElement.TryGetProperty("errorCode", out var errorCodeElement)
            ? errorCodeElement.GetString()
            : null;

        if (json.RootElement.TryGetProperty("confirmationToken", out var tokenElement))
            return (errorCode, tokenElement.GetString()!);

        if (json.RootElement.TryGetProperty("ConfirmationToken", out tokenElement))
            return (errorCode, tokenElement.GetString()!);

        throw new InvalidOperationException("Response did not contain a confirmation token.");
    }
}
