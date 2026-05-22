using System.Net;
using FitLead.Domain.Media.MediaAssets;
using FitLead.Application.Trainings.Workouts.Queries;
using FitLead.Domain.Trainings.Exercises;
using FitLead.IntegrationTests.Helpers;
using FitLead.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace FitLead.IntegrationTests.Features.Workouts;

[Collection(IntegrationTestCollectionNames.Default)]
public sealed class WorkoutDetailsTests : IntegrationTestBase
{
    private readonly TestUsers _users;
    private readonly TestExercises _exercises;
    private readonly TestMediaAssets _mediaAssets;
    private readonly TestApiClients _api;

    public WorkoutDetailsTests(IntegrationTestFixture fixture) : base(fixture)
    {
        var db = new TestDb(fixture);
        _users = new TestUsers(fixture, db);
        _exercises = new TestExercises(db);
        _mediaAssets = new TestMediaAssets(db);
        _api = new TestApiClients(fixture);
    }

    [Fact]
    public async Task GetDetails_WithPlatformAndOwnExercise_ShouldReturnOrderedExercisesWithPrescription()
    {
        var trainer = await _users.RegisterTrainerAsync("workout-details");
        var client = await _api.WorkoutsAsync(trainer.Auth);
        var platformMedia = await _mediaAssets.CreateAsync(trainer.Id, MediaAssetKind.Image);
        var ownMedia = await _mediaAssets.CreateAsync(trainer.Id, MediaAssetKind.Video);
        var platformExerciseId = await _exercises.CreatePlatformExerciseAsync(
            name: "Присідання",
            description: "Базова вправа для ніг",
            mediaAssetId: platformMedia.Id,
            muscleGroup: MuscleGroup.Legs,
            equipment: Equipment.Barbell);
        var ownExerciseId = await _exercises.CreateTrainerExerciseAsync(
            trainer.Id,
            name: "Планка",
            description: "Статична вправа для корпусу",
            mediaAssetId: ownMedia.Id,
            muscleGroup: MuscleGroup.Core,
            equipment: Equipment.Bodyweight);

        var createResponse = await client.CreateAsync("Full Body A");
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var workoutId = await createResponse.ReadRequiredJsonAsync<Guid>();

        var addPlatformResponse = await client.AddExerciseAsync(
            workoutId,
            platformExerciseId,
            repetitions: 8,
            sets: 4,
            loadKg: 60,
            restSeconds: 120,
            trainerNote: "Контролюй глибину");
        var addOwnResponse = await client.AddExerciseAsync(
            workoutId,
            ownExerciseId,
            repetitions: 45,
            sets: 3,
            loadKg: null,
            restSeconds: 60,
            trainerNote: "Тримай нейтральну спину");

        addPlatformResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        addOwnResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await client.GetDetailsAsync(workoutId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var details = await response.ReadRequiredJsonAsync<WorkoutDetailsDto>();
        details.Name.Should().Be("Full Body A");
        details.TrainerId.Should().Be(trainer.Id);
        details.Exercises.Should().HaveCount(2);

        var firstExercise = details.Exercises[0];
        firstExercise.ExerciseId.Should().Be(platformExerciseId);
        firstExercise.Order.Should().Be(1);
        firstExercise.ExerciseName.Should().Be("Присідання");
        firstExercise.ExerciseDescription.Should().Be("Базова вправа для ніг");
        firstExercise.ExerciseMediaAsset.Should().NotBeNull();
        firstExercise.ExerciseMediaAsset!.Id.Should().Be(platformMedia.Id);
        firstExercise.ExerciseMuscleGroup.Should().Be(MuscleGroup.Legs);
        firstExercise.ExerciseEquipment.Should().Be(Equipment.Barbell);
        firstExercise.Repetitions.Should().Be(8);
        firstExercise.Sets.Should().Be(4);
        firstExercise.LoadKg.Should().Be(60);
        firstExercise.RestSeconds.Should().Be(120);
        firstExercise.TrainerNote.Should().Be("Контролюй глибину");

        var secondExercise = details.Exercises[1];
        secondExercise.ExerciseId.Should().Be(ownExerciseId);
        secondExercise.Order.Should().Be(2);
        secondExercise.ExerciseName.Should().Be("Планка");
        secondExercise.ExerciseDescription.Should().Be("Статична вправа для корпусу");
        secondExercise.ExerciseMediaAsset.Should().NotBeNull();
        secondExercise.ExerciseMediaAsset!.Id.Should().Be(ownMedia.Id);
        secondExercise.ExerciseMuscleGroup.Should().Be(MuscleGroup.Core);
        secondExercise.ExerciseEquipment.Should().Be(Equipment.Bodyweight);
        secondExercise.Repetitions.Should().Be(45);
        secondExercise.Sets.Should().Be(3);
        secondExercise.LoadKg.Should().BeNull();
        secondExercise.RestSeconds.Should().Be(60);
        secondExercise.TrainerNote.Should().Be("Тримай нейтральну спину");
    }
}
