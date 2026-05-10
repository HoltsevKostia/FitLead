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

            var exercises = await query
                .OrderBy(x => x.Source)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);

            return exercises
                .Select(x => new ExerciseDto(
                    x.Id,
                    x.Name,
                    x.Description,
                    x.MediaUrl?.Value,
                    x.MuscleGroup,
                    x.Equipment,
                    x.Source,
                    x.CopiedFromExerciseId,
                    x.Source == ExerciseSource.Trainer &&
                    x.OwnerTrainerId == trainerId))
                .ToList();
        }

        public async Task<ExerciseDto?> GetVisibleByIdForTrainerAsync(
            Guid exerciseId,
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            var exercise = await _context.Exercises
                .AsNoTracking()
                .Where(x =>
                    x.Id == exerciseId &&
                    (x.Source == ExerciseSource.Platform ||
                     (x.Source == ExerciseSource.Trainer &&
                      x.OwnerTrainerId == trainerId)))
                .FirstOrDefaultAsync(cancellationToken);

            if (exercise is null)
            {
                return null;
            }

            return new ExerciseDto(
                exercise.Id,
                exercise.Name,
                exercise.Description,
                exercise.MediaUrl?.Value,
                exercise.MuscleGroup,
                exercise.Equipment,
                exercise.Source,
                exercise.CopiedFromExerciseId,
                exercise.Source == ExerciseSource.Trainer &&
                exercise.OwnerTrainerId == trainerId);
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
