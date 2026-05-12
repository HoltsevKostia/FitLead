using FitLead.Application.Trainings.TrainingPrograms.Queries;

namespace FitLead.Application.Trainings.TrainingProgramAssignments.Queries
{
    public sealed record ClientAssignedTrainingProgramDetailsDto(
        Guid AssignmentId,
        Guid ProgramId,
        string Title,
        Guid TrainerId,
        string TrainerName,
        int WeeksCount,
        int DaysPerWeek,
        DateTime AssignedAtUtc,
        DateTime? ExpiresAtUtc,
        IReadOnlyList<TrainingProgramWorkoutDto> Workouts);
}
