namespace FitLead.Application.Trainings.Workouts.Queries
{
    public sealed record WorkoutDto(
        Guid Id,
        string Name,
        Guid TrainerId
    );
}
