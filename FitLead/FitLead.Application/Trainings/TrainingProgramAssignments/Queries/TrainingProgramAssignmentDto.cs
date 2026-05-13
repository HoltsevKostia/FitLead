namespace FitLead.Application.Trainings.TrainingProgramAssignments.Queries
{
    public sealed record TrainingProgramAssignmentDto(
        Guid AssignmentId,
        Guid ClientId,
        string ClientName,
        string Status,
        string AccessSource,
        DateTime AssignedAtUtc,
        DateTime? ExpiresAtUtc,
        DateTime? RevokedAtUtc);
}
