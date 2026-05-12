namespace FitLead.Api.TrainingPrograms.Contracts
{
    public sealed record ReorderProgramWorkoutsRequest(
        int WeekNumber,
        int DayNumber,
        IReadOnlyList<Guid> EntryIds);
}
