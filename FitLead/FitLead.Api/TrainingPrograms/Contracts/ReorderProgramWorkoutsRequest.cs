namespace FitLead.Api.TrainingPrograms.Contracts
{
    public sealed record ReorderProgramWorkoutsRequest(IReadOnlyList<Guid> WorkoutIds);
}
