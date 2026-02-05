namespace FitLead.Application.Trainings.Workouts.Queries
{
    public sealed record WorkoutDetailsDto(
        Guid Id,
        Guid TrainerId,
        string Name,
        IReadOnlyList<WorkoutExerciseDetailsDto> Exercises
    );
}
