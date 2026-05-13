namespace FitLead.Api.TrainingPrograms.Contracts
{
    public sealed record AssignTrainingProgramToClientRequest(
        Guid ClientId,
        DateTime? ExpiresAtUtc);
}
