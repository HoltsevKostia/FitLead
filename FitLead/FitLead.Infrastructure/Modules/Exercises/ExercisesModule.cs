using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Modules.Exercises;

namespace FitLead.Infrastructure.Modules.Exercises
{
    public sealed class ExercisesModule : IExercisesModule
    {
        private readonly IExerciseRepository _exerciseRepository;

        public ExercisesModule(IExerciseRepository exerciseRepository)
        {
            _exerciseRepository = exerciseRepository;
        }

        public async Task<ExerciseModuleDescriptor?> GetByIdAsync(
            Guid exerciseId,
            CancellationToken cancellationToken = default)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(exerciseId, cancellationToken);
            if (exercise is null)
                return null;

            return new ExerciseModuleDescriptor(
                exercise.Id,
                exercise.OwnerTrainerId,
                exercise.Source);
        }

        public Task<bool> ExistsAsync(
            Guid exerciseId,
            CancellationToken cancellationToken = default)
        {
            return _exerciseRepository.ExistsAsync(exerciseId, cancellationToken);
        }
    }
}
