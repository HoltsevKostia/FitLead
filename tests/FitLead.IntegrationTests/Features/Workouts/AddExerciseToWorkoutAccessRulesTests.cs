using System.Net;
using FitLead.Domain.Trainings;
using FitLead.Infrastructure.Persistence;
using FitLead.IntegrationTests.Clients;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitLead.IntegrationTests.Features.Workouts;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class AddExerciseToWorkoutAccessRulesTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task AddExercise_WithOwnExercise_ShouldAddExerciseToWorkout()
    {
        var trainer = await RegisterTrainerAsync("workout-add-own");
        var trainerId = await GetDomainUserIdAsync(trainer.Email);
        var workoutId = await AddWorkoutAsync(trainerId);
        var exerciseId = await AddTrainerExerciseAsync(trainerId);
        var client = await CreateWorkoutsClientAsync(trainer.Auth);

        var response = await client.AddExerciseAsync(workoutId, exerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        await AssertWorkoutContainsExerciseAsync(workoutId, exerciseId);
    }

    [Fact]
    public async Task AddExercise_WithPlatformExercise_ShouldAddExerciseToWorkout()
    {
        var trainer = await RegisterTrainerAsync("workout-add-platform");
        var trainerId = await GetDomainUserIdAsync(trainer.Email);
        var workoutId = await AddWorkoutAsync(trainerId);
        var exerciseId = await AddPlatformExerciseAsync();
        var client = await CreateWorkoutsClientAsync(trainer.Auth);

        var response = await client.AddExerciseAsync(workoutId, exerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        await AssertWorkoutContainsExerciseAsync(workoutId, exerciseId);
    }

    [Fact]
    public async Task AddExercise_WithAnotherTrainerExercise_ShouldReturnNotFound()
    {
        var trainer = await RegisterTrainerAsync("workout-add-other-current");
        var otherTrainer = await RegisterTrainerAsync("workout-add-other-owner");
        var trainerId = await GetDomainUserIdAsync(trainer.Email);
        var otherTrainerId = await GetDomainUserIdAsync(otherTrainer.Email);
        var workoutId = await AddWorkoutAsync(trainerId);
        var otherExerciseId = await AddTrainerExerciseAsync(otherTrainerId);
        var client = await CreateWorkoutsClientAsync(trainer.Auth);

        var response = await client.AddExerciseAsync(workoutId, otherExerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.not_found");

        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
        var exists = await dbContext.WorkoutExercises
            .AnyAsync(x => EF.Property<Guid>(x, "workout_id") == workoutId && x.ExerciseId == otherExerciseId);
        exists.Should().BeFalse();
    }

    private async Task<(AuthTestClient Auth, string Email)> RegisterTrainerAsync(string prefix)
    {
        var auth = new AuthTestClient(Fixture.CreateClient(handleCookies: false));
        var email = UniqueEmail(prefix);

        var response = await auth.RegisterAsync(
            email,
            "Str0ngPass!123",
            "Workout Trainer",
            AuthRoles.Trainer);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (auth, email);
    }

    private async Task<WorkoutsTestClient> CreateWorkoutsClientAsync(AuthTestClient auth)
    {
        var client = new WorkoutsTestClient(Fixture.CreateClient(handleCookies: false));
        await client.CopyAuthStateFromAsync(auth);
        return client;
    }

    private async Task<Guid> AddWorkoutAsync(Guid trainerId)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();

        var workout = Workout.Create($"Тренування {Guid.NewGuid():N}", trainerId).Value;
        dbContext.Workouts.Add(workout);
        await dbContext.SaveChangesAsync();

        return workout.Id;
    }

    private async Task<Guid> AddTrainerExerciseAsync(Guid trainerId)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();

        var exercise = Exercise.CreateTrainerExercise(
            trainerId,
            $"Власна вправа {Guid.NewGuid():N}",
            "Опис власної вправи").Value;

        dbContext.Exercises.Add(exercise);
        await dbContext.SaveChangesAsync();

        return exercise.Id;
    }

    private async Task<Guid> AddPlatformExerciseAsync()
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();

        var exercise = Exercise.CreatePlatformExercise(
            $"Платформна вправа {Guid.NewGuid():N}",
            "Опис платформної вправи").Value;

        dbContext.Exercises.Add(exercise);
        await dbContext.SaveChangesAsync();

        return exercise.Id;
    }

    private async Task<Guid> GetDomainUserIdAsync(string email)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();

        return await dbContext.DomainUsers
            .Where(x => x.Email == email)
            .Select(x => x.Id)
            .SingleAsync();
    }

    private async Task AssertWorkoutContainsExerciseAsync(Guid workoutId, Guid exerciseId)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();

        var exists = await dbContext.WorkoutExercises
            .AnyAsync(x => EF.Property<Guid>(x, "workout_id") == workoutId && x.ExerciseId == exerciseId);
        exists.Should().BeTrue();
    }
}
