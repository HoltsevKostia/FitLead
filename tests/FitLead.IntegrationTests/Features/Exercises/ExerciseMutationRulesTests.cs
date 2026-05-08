using System.Net;
using FitLead.Domain.Trainings;
using FitLead.Infrastructure.Persistence;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.Exercises;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class ExerciseMutationRulesTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Update_WithOwnExercise_ShouldUpdateExercise()
    {
        var trainer = await RegisterTrainerAsync("exercise-update-own");
        var exerciseId = await AddTrainerExerciseAsync(trainer.Email, "Власна вправа");
        var client = await CreateExercisesClientAsync(trainer.Auth);

        var response = await client.UpdateAsync(
            exerciseId,
            name: "Оновлена власна вправа",
            description: "Новий опис");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
        var exercise = await dbContext.Exercises.SingleAsync(x => x.Id == exerciseId);
        exercise.Name.Should().Be("Оновлена власна вправа");
        exercise.Description.Should().Be("Новий опис");
    }

    [Fact]
    public async Task Update_WithPlatformExercise_ShouldReturnNotFound()
    {
        var trainer = await RegisterTrainerAsync("exercise-update-platform");
        var platformExerciseId = await AddPlatformExerciseAsync("Платформна вправа");
        var client = await CreateExercisesClientAsync(trainer.Auth);

        var response = await client.UpdateAsync(platformExerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.not_found");
    }

    [Fact]
    public async Task Update_WithAnotherTrainerExercise_ShouldReturnNotFound()
    {
        var trainer = await RegisterTrainerAsync("exercise-update-trainer");
        var otherTrainer = await RegisterTrainerAsync("exercise-update-other");
        var otherExerciseId = await AddTrainerExerciseAsync(otherTrainer.Email, "Чужа вправа");
        var client = await CreateExercisesClientAsync(trainer.Auth);

        var response = await client.UpdateAsync(otherExerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.not_found");
    }

    [Fact]
    public async Task Delete_WithOwnUnusedExercise_ShouldDeleteExercise()
    {
        var trainer = await RegisterTrainerAsync("exercise-delete-own");
        var exerciseId = await AddTrainerExerciseAsync(trainer.Email, "Вправа для видалення");
        var client = await CreateExercisesClientAsync(trainer.Auth);

        var response = await client.DeleteAsync(exerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
        var exists = await dbContext.Exercises.AnyAsync(x => x.Id == exerciseId);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_WithPlatformExercise_ShouldReturnNotFound()
    {
        var trainer = await RegisterTrainerAsync("exercise-delete-platform");
        var platformExerciseId = await AddPlatformExerciseAsync("Платформна вправа для delete");
        var client = await CreateExercisesClientAsync(trainer.Auth);

        var response = await client.DeleteAsync(platformExerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.not_found");
    }

    [Fact]
    public async Task Delete_WithAnotherTrainerExercise_ShouldReturnNotFound()
    {
        var trainer = await RegisterTrainerAsync("exercise-delete-trainer");
        var otherTrainer = await RegisterTrainerAsync("exercise-delete-other");
        var otherExerciseId = await AddTrainerExerciseAsync(otherTrainer.Email, "Чужа вправа для delete");
        var client = await CreateExercisesClientAsync(trainer.Auth);

        var response = await client.DeleteAsync(otherExerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.not_found");
    }

    [Fact]
    public async Task Delete_WithUsedOwnExercise_ShouldRequireConfirmationAndConfirmDelete()
    {
        var trainer = await RegisterTrainerAsync("exercise-delete-used");
        var exerciseId = await AddUsedTrainerExerciseAsync(trainer.Email);
        var client = await CreateExercisesClientAsync(trainer.Auth);

        var dryRunResponse = await client.DeleteAsync(exerciseId);

        dryRunResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var (errorCode, token) = await ReadDeletionConflictAsync(dryRunResponse);
        errorCode.Should().Be("exercise.in_use");
        token.Should().NotBeNullOrWhiteSpace();

        var confirmResponse = await client.ConfirmDeleteAsync(exerciseId, token);

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
        (await dbContext.Exercises.AnyAsync(x => x.Id == exerciseId)).Should().BeFalse();
        (await dbContext.WorkoutExercises.AnyAsync(x => x.ExerciseId == exerciseId)).Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmDelete_WithPlatformExercise_ShouldReturnNotFound()
    {
        var trainer = await RegisterTrainerAsync("exercise-confirm-platform");
        var platformExerciseId = await AddPlatformExerciseAsync("Платформна вправа для confirm");
        var client = await CreateExercisesClientAsync(trainer.Auth);

        var response = await client.ConfirmDeleteAsync(platformExerciseId, "invalid-token");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.not_found");
    }

    private async Task<(AuthTestClient Auth, string Email)> RegisterTrainerAsync(string prefix)
    {
        var auth = new AuthTestClient(Fixture.CreateClient(handleCookies: false));
        var email = UniqueEmail(prefix);

        var response = await auth.RegisterAsync(
            email,
            "Str0ngPass!123",
            "Exercise Trainer",
            AuthRoles.Trainer);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (auth, email);
    }

    private async Task<ExercisesTestClient> CreateExercisesClientAsync(AuthTestClient auth)
    {
        var client = new ExercisesTestClient(Fixture.CreateClient(handleCookies: false));
        await client.CopyAuthStateFromAsync(auth);
        return client;
    }

    private async Task<Guid> AddTrainerExerciseAsync(string trainerEmail, string name)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
        var trainerId = await GetDomainUserIdAsync(dbContext, trainerEmail);

        var exercise = Exercise.CreateTrainerExercise(
            trainerId,
            name,
            "Опис вправи",
            muscleGroup: MuscleGroup.Core,
            equipment: Equipment.Bodyweight).Value;

        dbContext.Exercises.Add(exercise);
        await dbContext.SaveChangesAsync();

        return exercise.Id;
    }

    private async Task<Guid> AddPlatformExerciseAsync(string name)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();

        var exercise = Exercise.CreatePlatformExercise(
            name,
            "Опис платформної вправи",
            muscleGroup: MuscleGroup.FullBody,
            equipment: Equipment.Bodyweight).Value;

        dbContext.Exercises.Add(exercise);
        await dbContext.SaveChangesAsync();

        return exercise.Id;
    }

    private async Task<Guid> AddUsedTrainerExerciseAsync(string trainerEmail)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
        var trainerId = await GetDomainUserIdAsync(dbContext, trainerEmail);

        var exercise = Exercise.CreateTrainerExercise(
            trainerId,
            "Використана вправа",
            "Опис використаної вправи").Value;

        var workout = Workout.Create("Тренування з використаною вправою", trainerId).Value;
        workout.AddExercise(exercise.Id, repetitions: 10, sets: 3, restSeconds: 60);

        dbContext.Exercises.Add(exercise);
        dbContext.Workouts.Add(workout);
        await dbContext.SaveChangesAsync();

        return exercise.Id;
    }

    private static async Task<Guid> GetDomainUserIdAsync(
        FitLeadDbContext dbContext,
        string email)
    {
        return await dbContext.DomainUsers
            .Where(x => x.Email == email)
            .Select(x => x.Id)
            .SingleAsync();
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
