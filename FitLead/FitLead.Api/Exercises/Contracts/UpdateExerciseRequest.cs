namespace FitLead.Api.Exercises.Contracts
{
    public sealed record UpdateExerciseRequest(
        string Name,
        string Description,
        string? MediaUrl
    );
}
