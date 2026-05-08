using FitLead.Application.Trainings.Exercises.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IExerciseReadRepository
    {
        Task<IReadOnlyList<ExerciseDto>> GetVisibleForTrainerAsync(
            Guid trainerId,
            ExerciseListSource source,
            CancellationToken cancellationToken);

        Task<int> GetUsageCountAsync(
            Guid exerciseId,
            CancellationToken cancellationToken);
    }
}
