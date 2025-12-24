namespace FitLead.Api.Contracts.Trainings
{
    public sealed record CreateExerciseRequest(
        Guid TrainerId,
        string Name,
        string Description,
        string? MediaUrl
    );
}
