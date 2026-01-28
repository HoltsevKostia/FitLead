namespace FitLead.Api.Contracts.Trainings
{
    public sealed record UpdateExerciseRequest(
        string Name,
        string Description,
        string? MediaUrl
    );
}
