using FitLead.Domain.Trainings.Exercises;

namespace FitLead.Application.Trainings.Workouts.Queries
{
    public sealed record WorkoutExerciseDetailsDto(
        Guid WorkoutExerciseId,
        Guid ExerciseId,
        int Order,
        string ExerciseName,
        string ExerciseDescription,
        string? ExerciseMediaUrl,
        MuscleGroup? ExerciseMuscleGroup,
        Equipment? ExerciseEquipment,
        int Repetitions,
        int Sets,
        decimal? LoadKg,
        int RestSeconds,
        string? TrainerNote
    );
}
