using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Trainings.Exercises.Queries;
using FitLead.Domain.Trainings.Exercises;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class ExerciseReadRepository : IExerciseReadRepository
    {
        private readonly FitLeadDbContext _context;

        public ExerciseReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<ExerciseDto>> GetVisibleForTrainerAsync(
            Guid trainerId,
            ExerciseListSource source,
            CancellationToken cancellationToken)
        {
            var query = _context.Exercises.AsNoTracking();

            query = source switch
            {
                ExerciseListSource.Platform => query
                    .Where(x => x.Source == ExerciseSource.Platform),

                ExerciseListSource.My => query
                    .Where(x =>
                        x.Source == ExerciseSource.Trainer &&
                        x.OwnerTrainerId == trainerId),

                ExerciseListSource.All => query
                    .Where(x =>
                        x.Source == ExerciseSource.Platform ||
                        (x.Source == ExerciseSource.Trainer &&
                         x.OwnerTrainerId == trainerId)),

                _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
            };

            var exercises = await (
                    from exercise in query
                    join mediaAsset in _context.MediaAssets.AsNoTracking()
                        on exercise.MediaAssetId equals mediaAsset.Id into mediaAssets
                    from mediaAsset in mediaAssets.DefaultIfEmpty()
                    orderby exercise.Source, exercise.Name, exercise.Id
                    select new
                    {
                        Exercise = exercise,
                        MediaAsset = mediaAsset
                    })
                .ToListAsync(cancellationToken);

            return exercises
                .Select(x => new ExerciseDto(
                    x.Exercise.Id,
                    x.Exercise.Name,
                    x.Exercise.Description,
                    MediaAssetProjectionMapper.ToPreviewDto(x.MediaAsset),
                    x.Exercise.MuscleGroup,
                    x.Exercise.Equipment,
                    x.Exercise.Source,
                    x.Exercise.CopiedFromExerciseId,
                    x.Exercise.Source == ExerciseSource.Trainer &&
                    x.Exercise.OwnerTrainerId == trainerId))
                .ToList();
        }

        public async Task<ExerciseDto?> GetVisibleByIdForTrainerAsync(
            Guid exerciseId,
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            var exercise = await (
                    from visibleExercise in _context.Exercises.AsNoTracking()
                    where visibleExercise.Id == exerciseId &&
                          (visibleExercise.Source == ExerciseSource.Platform ||
                           (visibleExercise.Source == ExerciseSource.Trainer &&
                            visibleExercise.OwnerTrainerId == trainerId))
                    join mediaAsset in _context.MediaAssets.AsNoTracking()
                        on visibleExercise.MediaAssetId equals mediaAsset.Id into mediaAssets
                    from mediaAsset in mediaAssets.DefaultIfEmpty()
                    select new
                    {
                        Exercise = visibleExercise,
                        MediaAsset = mediaAsset
                    })
                .FirstOrDefaultAsync(cancellationToken);

            if (exercise is null)
            {
                return null;
            }

            return new ExerciseDto(
                exercise.Exercise.Id,
                exercise.Exercise.Name,
                exercise.Exercise.Description,
                MediaAssetProjectionMapper.ToPreviewDto(exercise.MediaAsset),
                exercise.Exercise.MuscleGroup,
                exercise.Exercise.Equipment,
                exercise.Exercise.Source,
                exercise.Exercise.CopiedFromExerciseId,
                exercise.Exercise.Source == ExerciseSource.Trainer &&
                exercise.Exercise.OwnerTrainerId == trainerId);
        }

        public async Task<int> GetUsageCountAsync(
            Guid exerciseId,
            CancellationToken cancellationToken)
        {
            return await _context.WorkoutExercises
                .CountAsync(x => x.ExerciseId == exerciseId, cancellationToken);
        }
    }
}
