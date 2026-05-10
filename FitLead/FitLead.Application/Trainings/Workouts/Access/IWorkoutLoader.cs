using FitLead.Common.Results;
using FitLead.Domain.Trainings.Workouts;

namespace FitLead.Application.Trainings.Workouts.Access
{
    public interface IWorkoutLoader
    {
        Task<Result<Workout>> GetOwnedOrNotFoundAsync(
            Guid workoutId,
            CancellationToken cancellationToken);
    }
}
