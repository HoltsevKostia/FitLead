using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Trainings.Exercises.Queries;
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

        public async Task<IReadOnlyList<ExerciseDto>> GetByTrainerIdAsync(
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            var exercises = await _context.Exercises
                .Where(x => x.OwnerTrainerId == trainerId)
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            return exercises
                .Select(x => new ExerciseDto(
                    x.Id,
                    x.Name,
                    x.Description,
                    x.MediaUrl?.Value))
                .ToList();
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
