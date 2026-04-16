namespace FitLead.Api.Contracts.Trainings
{
    public sealed record AddWorkoutToProgramRequest(
        Guid WorkoutId
    );
}
