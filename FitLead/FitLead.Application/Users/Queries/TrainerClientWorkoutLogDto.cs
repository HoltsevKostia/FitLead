namespace FitLead.Application.Users.Queries
{
    public sealed record TrainerClientWorkoutLogDto(
        Guid LogId,
        Guid AssignmentId,
        Guid ProgramId,
        string ProgramTitle,
        Guid ProgramWorkoutId,
        Guid WorkoutId,
        string WorkoutName,
        int WeekNumber,
        int DayNumber,
        int OrderInDay,
        string Status,
        DateTime? PerformedAtUtc,
        int? DifficultyRating,
        string? ClientNote,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc);
}
