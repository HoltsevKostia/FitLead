namespace FitLead.Application.Trainings.TrainingProgramAssignments.Commands
{
    public sealed record AssignTrainingProgramToClientResult(
        Guid AssignmentId,
        Guid ProgramId,
        Guid ClientId,
        string Status,
        string AccessSource,
        DateTime AssignedAtUtc,
        DateTime? ExpiresAtUtc);
}
