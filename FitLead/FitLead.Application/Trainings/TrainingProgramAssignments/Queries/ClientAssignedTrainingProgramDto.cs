namespace FitLead.Application.Trainings.TrainingProgramAssignments.Queries
{
    public sealed record ClientAssignedTrainingProgramDto(
        Guid AssignmentId,
        Guid ProgramId,
        string Title,
        Guid TrainerId,
        string TrainerName,
        int WeeksCount,
        int DaysPerWeek,
        DateTime AssignedAtUtc,
        DateTime? ExpiresAtUtc);
}
