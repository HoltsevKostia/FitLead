namespace FitLead.Application.ClientDashboard.Queries
{
    public sealed record ClientDashboardDto(
        ClientDashboardTrainerDto? Trainer,
        IReadOnlyList<ClientDashboardProgramDto> ActivePrograms);

    public sealed record ClientDashboardTrainerDto(
        Guid TrainerId,
        string FullName);

    public sealed record ClientDashboardProgramDto(
        Guid AssignmentId,
        Guid ProgramId,
        string Title,
        int WeeksCount,
        int DaysPerWeek,
        DateTime AssignedAtUtc,
        DateTime? ExpiresAtUtc,
        int CompletedCount,
        int SkippedCount,
        int PendingCount,
        ClientDashboardNextWorkoutDto? NextWorkout);

    public sealed record ClientDashboardNextWorkoutDto(
        Guid ProgramWorkoutId,
        Guid WorkoutId,
        string WorkoutName,
        int WeekNumber,
        int DayNumber,
        int OrderInDay);
}
