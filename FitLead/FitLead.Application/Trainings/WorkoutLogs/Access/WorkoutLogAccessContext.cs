namespace FitLead.Application.Trainings.WorkoutLogs.Access
{
    public sealed record WorkoutLogAccessContext(
        Guid AssignedTrainingProgramId,
        Guid TrainingProgramWorkoutId,
        Guid ClientId,
        Guid TrainerId);
}
