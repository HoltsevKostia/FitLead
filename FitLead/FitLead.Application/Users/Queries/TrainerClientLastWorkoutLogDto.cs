namespace FitLead.Application.Users.Queries
{
    public sealed record TrainerClientLastWorkoutLogDto(
        Guid Id,
        Guid AssignmentId,
        Guid ProgramWorkoutId,
        string ProgramTitle,
        string WorkoutName,
        int WeekNumber,
        int DayNumber,
        int OrderInDay,
        string Status,
        DateTime? PerformedAtUtc,
        string? ClientNote,
        int? DifficultyRating,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc);
}
