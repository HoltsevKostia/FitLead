namespace FitLead.Api.TrainingPrograms.Contracts
{
    public sealed record CreateTrainingProgramRequest(
        string Title,
        int WeeksCount,
        int DaysPerWeek);
}
