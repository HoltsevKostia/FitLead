using FitLead.Domain.Trainings.Exercises;
using FluentAssertions;

namespace FitLead.IntegrationTests.Unit.Trainings;

public sealed class ExerciseOwnershipTests
{
    [Fact]
    public void CreateTrainerExercise_WithoutOwnerTrainerId_ShouldReturnFailure()
    {
        var result = Exercise.CreateTrainerExercise(
            Guid.Empty,
            "Squat",
            "Bodyweight squat");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("exercise.ownership.trainer_owner_required");
    }

    [Fact]
    public void CopyFromPlatformExercise_WithTrainerExerciseSource_ShouldReturnFailure()
    {
        var trainerExercise = Exercise.CreateTrainerExercise(
            Guid.NewGuid(),
            "Squat",
            "Bodyweight squat").Value;

        var result = Exercise.CopyFromPlatformExercise(
            Guid.NewGuid(),
            trainerExercise);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("exercise.copy.source_must_be_platform");
    }

    [Fact]
    public void CopyFromPlatformExercise_WithPlatformExercise_ShouldCreateTrainerCopy()
    {
        var trainerId = Guid.NewGuid();
        var platformExercise = Exercise.CreatePlatformExercise(
            "Squat",
            "Bodyweight squat").Value;

        var result = Exercise.CopyFromPlatformExercise(trainerId, platformExercise);

        result.IsSuccess.Should().BeTrue();
        result.Value.Source.Should().Be(ExerciseSource.Trainer);
        result.Value.OwnerTrainerId.Should().Be(trainerId);
        result.Value.CopiedFromExerciseId.Should().Be(platformExercise.Id);
    }
}
