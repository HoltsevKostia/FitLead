namespace FitLead.Api.Workouts.Contracts
{
    public sealed record UpdateWorkoutExerciseRequest(
        int Repetitions,
        int Sets,
        decimal? LoadKg,
        int RestSeconds,
        string? TrainerNote
    );
}
