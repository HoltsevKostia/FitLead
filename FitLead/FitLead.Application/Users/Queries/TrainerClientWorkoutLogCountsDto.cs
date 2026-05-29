namespace FitLead.Application.Users.Queries
{
    public sealed record TrainerClientWorkoutLogCountsDto(
        int Completed,
        int Skipped,
        int Pending);
}
