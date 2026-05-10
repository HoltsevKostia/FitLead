using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Trainings.Exercises;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class ExerciseRepository : IExerciseRepository
    {
        private readonly FitLeadDbContext _context;

        public ExerciseRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            Exercise exercise,
            CancellationToken cancellationToken)
        {
            await _context.Exercises.AddAsync(exercise, cancellationToken);
        }

        public async Task<Exercise?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _context.Exercises
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken)
        {
            return await _context.Exercises
                .AnyAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> TrainerCopyExistsAsync(
            Guid ownerTrainerId,
            Guid copiedFromExerciseId,
            CancellationToken cancellationToken)
        {
            return await _context.Exercises.AnyAsync(
                x => x.OwnerTrainerId == ownerTrainerId &&
                    x.CopiedFromExerciseId == copiedFromExerciseId,
                cancellationToken);
        }

        public async Task DeleteWorkoutExercisesByExerciseIdAsync(
            Guid exerciseId,
            CancellationToken cancellationToken)
        {
            await _context.WorkoutExercises
                .Where(x => x.ExerciseId == exerciseId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public void Remove(Exercise exercise)
        {
            _context.Exercises.Remove(exercise);
        }
    }
}
