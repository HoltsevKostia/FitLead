using System.Net;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Workouts;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class AddExerciseToWorkoutAccessRulesTests : IntegrationTestBase
{
    private readonly TestUsers _users;
    private readonly TestExercises _exercises;
    private readonly TestWorkouts _workouts;
    private readonly TestApiClients _api;

    public AddExerciseToWorkoutAccessRulesTests(IntegrationTestFixture fixture) : base(fixture)
    {
        var db = new TestDb(fixture);
        _users = new TestUsers(fixture, db);
        _exercises = new TestExercises(db);
        _workouts = new TestWorkouts(db);
        _api = new TestApiClients(fixture);
    }

    [Fact]
    public async Task AddExercise_WithOwnExercise_ShouldAddExerciseToWorkout()
    {
        var trainer = await _users.RegisterTrainerAsync("workout-add-own");
        var workoutId = await _workouts.CreateWorkoutAsync(trainer.Id);
        var exerciseId = await _exercises.CreateTrainerExerciseAsync(trainer.Id);
        var client = await _api.WorkoutsAsync(trainer.Auth);

        var response = await client.AddExerciseAsync(workoutId, exerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await _workouts.ContainsExerciseAsync(workoutId, exerciseId)).Should().BeTrue();
    }

    [Fact]
    public async Task AddExercise_WithPlatformExercise_ShouldAddExerciseToWorkout()
    {
        var trainer = await _users.RegisterTrainerAsync("workout-add-platform");
        var workoutId = await _workouts.CreateWorkoutAsync(trainer.Id);
        var exerciseId = await _exercises.CreatePlatformExerciseAsync();
        var client = await _api.WorkoutsAsync(trainer.Auth);

        var response = await client.AddExerciseAsync(workoutId, exerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await _workouts.ContainsExerciseAsync(workoutId, exerciseId)).Should().BeTrue();
    }

    [Fact]
    public async Task AddExercise_WithAnotherTrainerExercise_ShouldReturnNotFound()
    {
        var trainer = await _users.RegisterTrainerAsync("workout-add-other-current");
        var otherTrainer = await _users.RegisterTrainerAsync("workout-add-other-owner");
        var workoutId = await _workouts.CreateWorkoutAsync(trainer.Id);
        var otherExerciseId = await _exercises.CreateTrainerExerciseAsync(otherTrainer.Id);
        var client = await _api.WorkoutsAsync(trainer.Auth);

        var response = await client.AddExerciseAsync(workoutId, otherExerciseId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.ReadProblemDetailsAsync();
        problem.ErrorCode.Should().Be("exercise.not_found");
        (await _workouts.ContainsExerciseAsync(workoutId, otherExerciseId)).Should().BeFalse();
    }
}
