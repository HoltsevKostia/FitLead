namespace FitLead.Api.Exercises.Contracts
{
    public sealed record AddExerciseToWorkoutRequest(
    Guid ExerciseId,
    int Repetitions,
    int Sets,
    decimal? LoadKg,
    int RestSeconds,
    string? TrainerNote);
}
