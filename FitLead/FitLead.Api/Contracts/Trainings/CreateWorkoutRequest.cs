namespace FitLead.Api.Contracts.Trainings
{
    public sealed record CreateWorkoutRequest(
    Guid TrainerId,
    string Name);
}
