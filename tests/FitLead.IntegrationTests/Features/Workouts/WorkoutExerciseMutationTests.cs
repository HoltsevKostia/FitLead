using System.Net;
using FitLead.Application.Trainings.Workouts.Queries;
using FitLead.Domain.Trainings.Exercises;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Workouts;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class WorkoutExerciseMutationTests : IntegrationTestBase
{
    private readonly TestUsers _users;
    private readonly TestExercises _exercises;
    private readonly TestApiClients _api;

    public WorkoutExerciseMutationTests(IntegrationTestFixture fixture) : base(fixture)
    {
        var db = new TestDb(fixture);
        _users = new TestUsers(fixture, db);
        _exercises = new TestExercises(db);
        _api = new TestApiClients(fixture);
    }

    [Fact]
    public async Task UpdateExercise_ShouldUpdateWorkoutExercisePrescription()
    {
        var trainer = await _users.RegisterTrainerAsync("workout-exercise-update");
        var client = await _api.WorkoutsAsync(trainer.Auth);
        var exerciseId = await _exercises.CreateTrainerExerciseAsync(
            trainer.Id,
            name: "Жим лежачи",
            description: "Вправа для грудних",
            muscleGroup: MuscleGroup.Chest,
            equipment: Equipment.Barbell);
        var createResponse = await client.CreateAsync("Push Day");
        var workoutId = await createResponse.ReadRequiredJsonAsync<Guid>();
        var addResponse = await client.AddExerciseAsync(
            workoutId,
            exerciseId,
            repetitions: 10,
            sets: 3,
            loadKg: 45,
            restSeconds: 90,
            trainerNote: "Початкові параметри");
        var workoutExerciseId = await addResponse.ReadRequiredJsonAsync<Guid>();

        var updateResponse = await client.UpdateExerciseAsync(
            workoutId,
            workoutExerciseId,
            repetitions: 8,
            sets: 4,
            loadKg: 60.5m,
            restSeconds: 120,
            trainerNote: "Контролюй паузу внизу");

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var detailsResponse = await client.GetDetailsAsync(workoutId);
        var details = await detailsResponse.ReadRequiredJsonAsync<WorkoutDetailsDto>();
        var workoutExercise = details.Exercises.Should().ContainSingle().Subject;
        workoutExercise.WorkoutExerciseId.Should().Be(workoutExerciseId);
        workoutExercise.Repetitions.Should().Be(8);
        workoutExercise.Sets.Should().Be(4);
        workoutExercise.LoadKg.Should().Be(60.5m);
        workoutExercise.RestSeconds.Should().Be(120);
        workoutExercise.TrainerNote.Should().Be("Контролюй паузу внизу");
    }

    [Fact]
    public async Task RemoveExercise_ShouldRemoveWorkoutExerciseAndKeepRemainingOrderValues()
    {
        var trainer = await _users.RegisterTrainerAsync("workout-exercise-remove");
        var client = await _api.WorkoutsAsync(trainer.Auth);
        var firstExerciseId = await _exercises.CreateTrainerExerciseAsync(
            trainer.Id,
            name: "Присідання",
            muscleGroup: MuscleGroup.Legs,
            equipment: Equipment.Barbell);
        var secondExerciseId = await _exercises.CreatePlatformExerciseAsync(
            name: "Планка",
            muscleGroup: MuscleGroup.Core,
            equipment: Equipment.Bodyweight);
        var createResponse = await client.CreateAsync("Full Body A");
        var workoutId = await createResponse.ReadRequiredJsonAsync<Guid>();
        var addFirstResponse = await client.AddExerciseAsync(workoutId, firstExerciseId);
        var addSecondResponse = await client.AddExerciseAsync(workoutId, secondExerciseId);
        var firstWorkoutExerciseId = await addFirstResponse.ReadRequiredJsonAsync<Guid>();
        var secondWorkoutExerciseId = await addSecondResponse.ReadRequiredJsonAsync<Guid>();

        var removeResponse = await client.RemoveExerciseAsync(
            workoutId,
            firstWorkoutExerciseId);

        removeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var detailsResponse = await client.GetDetailsAsync(workoutId);
        var details = await detailsResponse.ReadRequiredJsonAsync<WorkoutDetailsDto>();
        var remainingExercise = details.Exercises.Should().ContainSingle().Subject;
        remainingExercise.WorkoutExerciseId.Should().Be(secondWorkoutExerciseId);
        remainingExercise.ExerciseId.Should().Be(secondExerciseId);
        remainingExercise.Order.Should().Be(2);
    }
}
