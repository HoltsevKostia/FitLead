using FitLead.Domain.Trainings.Workouts;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IWorkoutRepository
    {
        Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task AddAsync(Workout workout, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
        Task<Guid?> GetTrainerIdAsync(Guid workoutId, CancellationToken cancellationToken);
        Task DeleteTrainingProgramWorkoutsByWorkoutIdAsync(Guid workoutId, CancellationToken cancellationToken);
        void Remove(Workout workout);
    }
}
