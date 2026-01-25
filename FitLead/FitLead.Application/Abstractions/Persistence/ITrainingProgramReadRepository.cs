using FitLead.Application.Trainings.TrainingPrograms.Queries;
using FitLead.Application.Trainings.Workouts.Queries;

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

        Task<IReadOnlyList<WorkoutDto>> GetWorkoutsByProgramIdAsync(
            Guid programId,
            CancellationToken cancellationToken);
    }
}
