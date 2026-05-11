using FitLead.Application.Trainings.TrainingPrograms.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainingProgramReadRepository
    {
        Task<IReadOnlyList<TrainingProgramDto>> GetByTrainerIdAsync(
            Guid trainerId,
            CancellationToken cancellationToken);

        Task<bool> IsOwnedByTrainerAsync(
            Guid programId,
            Guid trainerId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<TrainingProgramWorkoutDto>> GetWorkoutsByProgramIdAsync(
            Guid programId,
            CancellationToken cancellationToken);

        Task<int> GetUsageCountAsync(
            Guid programId,
            CancellationToken cancellationToken);
    }
}
