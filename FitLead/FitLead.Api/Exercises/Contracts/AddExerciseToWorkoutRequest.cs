namespace FitLead.Api.Exercises.Contracts
{
    public sealed record AddExerciseToWorkoutRequest(
    Guid ExerciseId,
    int Repetitions,
    int Sets,
    int RestSeconds);
}
