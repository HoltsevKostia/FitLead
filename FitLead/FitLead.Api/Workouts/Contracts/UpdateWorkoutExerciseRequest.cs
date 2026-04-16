namespace FitLead.Api.Workouts.Contracts
{
    public sealed record UpdateWorkoutExerciseRequest(
        int Repetitions,
        int Sets,
        int RestSeconds
    );
}
