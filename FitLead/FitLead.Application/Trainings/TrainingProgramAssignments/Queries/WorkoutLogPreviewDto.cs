namespace FitLead.Application.Trainings.TrainingProgramAssignments.Queries
{
    public sealed record WorkoutLogPreviewDto(
        Guid Id,
        string Status,
        DateTime? PerformedAtUtc,
        string? ClientNote,
        int? DifficultyRating,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc);
}
