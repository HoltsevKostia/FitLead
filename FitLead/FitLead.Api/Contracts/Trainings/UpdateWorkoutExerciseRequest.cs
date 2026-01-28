namespace FitLead.Api.Contracts.Trainings
{
    public sealed record UpdateWorkoutExerciseRequest(
        int Repetitions,
        int Sets,
        int RestSeconds
    );
}
