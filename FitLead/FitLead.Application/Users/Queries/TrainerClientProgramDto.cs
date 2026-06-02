namespace FitLead.Application.Users.Queries
{
    public sealed record TrainerClientProgramDto(
        Guid AssignmentId,
        Guid ProgramId,
        string ProgramTitle,
        string Status,
        DateTime AssignedAtUtc,
        DateTime? ExpiresAtUtc,
        DateTime? RevokedAtUtc,
        int TotalWorkouts,
        TrainerClientWorkoutLogCountsDto WorkoutLogCounts);
}
