

using FitLead.Application.Trainings.Queries.Workout;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IWorkoutReadRepository
    {
        Task<IReadOnlyList<WorkoutDto>> GetByTrainerIdAsync(
            Guid trainerId,
            CancellationToken cancellationToken);
    }
}
