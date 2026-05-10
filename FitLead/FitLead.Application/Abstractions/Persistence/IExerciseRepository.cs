using FitLead.Domain.Trainings.Exercises;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IExerciseRepository
    {
        Task AddAsync(Exercise exercise, CancellationToken cancellationToken);
        Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> TrainerCopyExistsAsync(
            Guid ownerTrainerId,
            Guid copiedFromExerciseId,
            CancellationToken cancellationToken);
        Task DeleteWorkoutExercisesByExerciseIdAsync(Guid exerciseId, CancellationToken cancellationToken);
        void Remove(Exercise exercise);
    }
}
