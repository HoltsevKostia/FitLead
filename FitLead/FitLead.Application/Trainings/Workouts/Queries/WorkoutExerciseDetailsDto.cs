using FitLead.Domain.Trainings;

namespace FitLead.Application.Trainings.Workouts.Queries
{
    public sealed record WorkoutExerciseDetailsDto(
        Guid WorkoutExerciseId,
        Guid ExerciseId,
        string ExerciseName,
        string ExerciseDescription,
        string? ExerciseMediaUrl,
        MuscleGroup? ExerciseMuscleGroup,
        Equipment? ExerciseEquipment,
        int Repetitions,
        int Sets,
        int RestSeconds
    );
}
