namespace FitLead.Application.Users.Queries
{
    public sealed record TrainerClientActiveProgramSummaryDto(
        Guid AssignmentId,
        Guid ProgramId,
        string ProgramTitle,
        DateTime AssignedAtUtc,
        DateTime? ExpiresAtUtc,
        int TotalWorkouts);
}
