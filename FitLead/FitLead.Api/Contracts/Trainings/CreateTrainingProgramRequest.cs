namespace FitLead.Api.Contracts.Trainings
{
    public record CreateTrainingProgramRequest(
    Guid TrainerId,
    string Title
);
}
