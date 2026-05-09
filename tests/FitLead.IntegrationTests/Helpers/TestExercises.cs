using FitLead.Domain.Trainings;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Helpers;

public sealed class TestExercises(TestDb db)
{
    public async Task<Guid> CreateTrainerExerciseAsync(
        Guid trainerId,
        string? name = null,
        string description = "Опис вправи",
        MuscleGroup? muscleGroup = MuscleGroup.Core,
        Equipment? equipment = Equipment.Bodyweight)
    {
        var exercise = Exercise.CreateTrainerExercise(
            trainerId,
            name ?? $"Власна вправа {Guid.NewGuid():N}",
            description,
            muscleGroup: muscleGroup,
            equipment: equipment).Value;

        await db.ExecuteAsync(async context =>
        {
            context.Exercises.Add(exercise);
            await context.SaveChangesAsync();
        });

        return exercise.Id;
    }

    public async Task<Guid> CreatePlatformExerciseAsync(
        string? name = null,
        string description = "Опис платформної вправи",
        MuscleGroup? muscleGroup = MuscleGroup.FullBody,
        Equipment? equipment = Equipment.Bodyweight)
    {
        var exercise = Exercise.CreatePlatformExercise(
            name ?? $"Платформна вправа {Guid.NewGuid():N}",
            description,
            muscleGroup: muscleGroup,
            equipment: equipment).Value;

        await db.ExecuteAsync(async context =>
        {
            context.Exercises.Add(exercise);
            await context.SaveChangesAsync();
        });

        return exercise.Id;
    }

    public async Task<Guid> CreateUsedTrainerExerciseAsync(Guid trainerId)
    {
        var exercise = Exercise.CreateTrainerExercise(
            trainerId,
            "Використана вправа",
            "Опис використаної вправи").Value;

        var workout = Workout.Create("Тренування з використаною вправою", trainerId).Value;
        workout.AddExercise(exercise.Id, repetitions: 10, sets: 3, restSeconds: 60);

        await db.ExecuteAsync(async context =>
        {
            context.Exercises.Add(exercise);
            context.Workouts.Add(workout);
            await context.SaveChangesAsync();
        });

        return exercise.Id;
    }

    public Task<Exercise> GetRequiredAsync(Guid exerciseId)
    {
        return db.QueryAsync(context =>
            context.Exercises.SingleAsync(x => x.Id == exerciseId));
    }

    public Task<bool> ExistsAsync(Guid exerciseId)
    {
        return db.QueryAsync(context =>
            context.Exercises.AnyAsync(x => x.Id == exerciseId));
    }
}
