namespace FitLead.Api.Contracts.Trainings
{
    public sealed record CreateExerciseRequest(
        string Name,
        string Description,
        string? MediaUrl
    );
}
