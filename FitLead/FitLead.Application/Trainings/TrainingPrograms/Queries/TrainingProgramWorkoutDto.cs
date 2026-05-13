namespace FitLead.Application.Trainings.TrainingPrograms.Queries
{
    public sealed record TrainingProgramWorkoutDto(
        Guid Id,
        Guid WorkoutId,
        string WorkoutName,
        Guid TrainerId,
        int WeekNumber,
        int DayNumber,
        int OrderInDay);
}
