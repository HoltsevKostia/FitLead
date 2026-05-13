namespace FitLead.Application.Users.Queries
{
    public sealed record TrainerClientProgramAccessDto(
        Guid AssignmentId,
        Guid ProgramId,
        string ProgramTitle,
        DateTime AssignedAtUtc,
        DateTime? ExpiresAtUtc);
}
