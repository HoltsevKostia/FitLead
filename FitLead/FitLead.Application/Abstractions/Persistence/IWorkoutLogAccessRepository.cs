using FitLead.Application.Trainings.WorkoutLogs.Access;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IWorkoutLogAccessRepository
    {
        Task<WorkoutLogAccessContext?> GetAccessibleForClientAsync(
            Guid assignmentId,
            Guid trainingProgramWorkoutId,
            Guid clientId,
            DateTime utcNow,
            CancellationToken cancellationToken);
    }
}
