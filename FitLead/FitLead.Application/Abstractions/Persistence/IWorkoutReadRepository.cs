using FitLead.Application.Trainings.Workouts.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IWorkoutReadRepository
    {
        Task<IReadOnlyList<WorkoutDto>> GetByTrainerIdAsync(
            Guid trainerId,
            CancellationToken cancellationToken);

        Task<WorkoutDetailsDto?> GetWorkoutDetailsByIdAsync(
            Guid workoutId,
            Guid trainerId,
            CancellationToken cancellationToken);
    }
}
