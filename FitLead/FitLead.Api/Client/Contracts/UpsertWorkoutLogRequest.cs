namespace FitLead.Api.Client.Contracts
{
    public sealed record UpsertWorkoutLogRequest(
        string? Status,
        DateTime? PerformedAtUtc,
        string? ClientNote,
        int? DifficultyRating);
}
