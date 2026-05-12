namespace FitLead.Api.TrainingPrograms.Contracts
{
    public sealed record MoveWorkoutEntryRequest(
        int TargetWeekNumber,
        int TargetDayNumber,
        int TargetOrderInDay);
}
