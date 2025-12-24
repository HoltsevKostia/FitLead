using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Trainings.Queries.Exercise;
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
            return await _context.Exercises
                .Where(x => x.TrainerId == trainerId)
                .OrderBy(x => x.Name)
                .Select(x => new ExerciseDto(
                    x.Id,
                    x.Name,
                    x.Description,
                    x.MediaUrl))
                .ToListAsync(cancellationToken);
        }
    }
}
