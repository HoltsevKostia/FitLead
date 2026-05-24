namespace FitLead.Application.Trainings.WorkoutLogs.Commands
{
    public sealed record WorkoutLogDto(
        Guid Id,
        string Status,
        DateTime? PerformedAtUtc,
        string? ClientNote,
        int? DifficultyRating,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc);
}
