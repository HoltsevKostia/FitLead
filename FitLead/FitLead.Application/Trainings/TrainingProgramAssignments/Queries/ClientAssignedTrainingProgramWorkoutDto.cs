using FitLead.Application.Trainings.Workouts.Queries;

namespace FitLead.Application.Trainings.TrainingProgramAssignments.Queries
{
    public sealed record ClientAssignedTrainingProgramWorkoutDto(
        Guid Id,
        Guid WorkoutId,
        string WorkoutName,
        Guid TrainerId,
        int WeekNumber,
        int DayNumber,
        int OrderInDay,
        IReadOnlyList<WorkoutExerciseDetailsDto> Exercises);
}
