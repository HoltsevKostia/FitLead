namespace FitLead.Api.Exercises.Contracts
{
    public sealed record CreateExerciseRequest(
        string Name,
        string Description,
        string? MediaUrl
    );
}
