using FitLead.Domain.Trainings.Workouts;
using Microsoft.EntityFrameworkCore;

namespace FitLead.IntegrationTests.Helpers;

public sealed class TestWorkouts(TestDb db)
{
    public async Task<Guid> CreateWorkoutAsync(
        Guid trainerId,
        string? name = null)
    {
        var workout = Workout.Create(
            name ?? $"Тренування {Guid.NewGuid():N}",
            trainerId).Value;

        await db.ExecuteAsync(async context =>
        {
            context.Workouts.Add(workout);
            await context.SaveChangesAsync();
        });

        return workout.Id;
    }

    public Task<bool> ContainsExerciseAsync(
        Guid workoutId,
        Guid exerciseId)
    {
        return db.QueryAsync(context =>
            context.WorkoutExercises.AnyAsync(x =>
                EF.Property<Guid>(x, "workout_id") == workoutId &&
                x.ExerciseId == exerciseId));
    }

    public Task<bool> HasAnyExerciseLinkAsync(Guid exerciseId)
    {
        return db.QueryAsync(context =>
            context.WorkoutExercises.AnyAsync(x => x.ExerciseId == exerciseId));
    }
}
