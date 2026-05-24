using FitLead.Domain.Trainings.WorkoutLogs;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IWorkoutLogRepository
    {
        Task AddAsync(WorkoutLog workoutLog, CancellationToken cancellationToken);

        Task<WorkoutLog?> GetByAssignmentWorkoutAsync(
            Guid assignedTrainingProgramId,
            Guid trainingProgramWorkoutId,
            CancellationToken cancellationToken);
    }
}
