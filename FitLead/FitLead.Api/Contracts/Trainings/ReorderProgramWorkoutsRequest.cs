namespace FitLead.Api.Contracts.Trainings
{
    public sealed record ReorderProgramWorkoutsRequest(IReadOnlyList<Guid> WorkoutIds);
}
