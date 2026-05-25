using FitLead.Common.Results;

namespace FitLead.Application.Trainings.WorkoutLogs.Access
{
    public interface IWorkoutLogAccessLoader
    {
        Task<Result<WorkoutLogAccessContext>> GetForCurrentClientOrNotFoundAsync(
            Guid assignmentId,
            Guid trainingProgramWorkoutId,
            CancellationToken cancellationToken);
    }
}
