using FitLead.Domain.Trainings.Workouts;
using FluentAssertions;

namespace FitLead.IntegrationTests.Unit.Trainings;

public sealed class WorkoutExercisePrescriptionTests
{
    [Fact]
    public void AddExercise_ShouldGenerateNextOrder()
    {
        var workout = Workout.Create("Full Body A", Guid.NewGuid()).Value;
        var firstExerciseId = Guid.NewGuid();
        var secondExerciseId = Guid.NewGuid();

        workout.AddExercise(
            firstExerciseId,
            repetitions: 8,
            sets: 4,
            loadKg: null,
            restSeconds: 120,
            trainerNote: null);
        workout.AddExercise(
            secondExerciseId,
            repetitions: 10,
            sets: 3,
            loadKg: null,
            restSeconds: 90,
            trainerNote: null);

        workout.Exercises.Single(x => x.ExerciseId == firstExerciseId).Order.Should().Be(1);
        workout.Exercises.Single(x => x.ExerciseId == secondExerciseId).Order.Should().Be(2);
    }

    [Fact]
    public void AddExercise_WithNegativeLoadKg_ShouldReturnFailure()
    {
        var workout = Workout.Create("Full Body A", Guid.NewGuid()).Value;

        var result = workout.AddExercise(
            Guid.NewGuid(),
            repetitions: 8,
            sets: 4,
            loadKg: -1,
            restSeconds: 120,
            trainerNote: null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("workout.exercise.create.load_kg_negative");
    }

    [Fact]
    public void AddExercise_WithTooLongTrainerNote_ShouldReturnFailure()
    {
        var workout = Workout.Create("Full Body A", Guid.NewGuid()).Value;
        var trainerNote = new string('a', WorkoutExercise.MaxTrainerNoteLength + 1);

        var result = workout.AddExercise(
            Guid.NewGuid(),
            repetitions: 8,
            sets: 4,
            loadKg: null,
            restSeconds: 120,
            trainerNote: trainerNote);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("workout.exercise.create.trainer_note_too_long");
    }

    [Fact]
    public void UpdateExercise_ShouldUpdatePrescriptionFields()
    {
        var workout = Workout.Create("Full Body A", Guid.NewGuid()).Value;
        var addResult = workout.AddExercise(
            Guid.NewGuid(),
            repetitions: 8,
            sets: 4,
            loadKg: null,
            restSeconds: 120,
            trainerNote: null);

        var updateResult = workout.UpdateExercise(
            addResult.Value,
            repetitions: 10,
            sets: 3,
            loadKg: 60.5m,
            restSeconds: 90,
            trainerNote: "  Контролюй темп  ");

        updateResult.IsSuccess.Should().BeTrue();
        var exercise = workout.Exercises.Single();
        exercise.Repetitions.Should().Be(10);
        exercise.Sets.Should().Be(3);
        exercise.LoadKg.Should().Be(60.5m);
        exercise.RestSeconds.Should().Be(90);
        exercise.TrainerNote.Should().Be("Контролюй темп");
    }
}
