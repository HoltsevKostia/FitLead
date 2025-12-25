namespace FitLead.Api.Contracts.Trainings
{
    public sealed record AddExerciseToWorkoutRequest(
    Guid ExerciseId,
    int Repetitions,
    int Sets,
    int RestSeconds);
}
