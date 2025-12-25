using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Trainings.Queries.Workout;
using Microsoft.EntityFrameworkCore;


namespace FitLead.Infrastructure.Persistence.Repositories
{
    internal sealed class WorkoutReadRepository : IWorkoutReadRepository
    {
        private readonly FitLeadDbContext _context;

        public WorkoutReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<WorkoutDto>> GetByTrainerIdAsync(
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            return await _context.Workouts
                .Where(x => x.TrainerId == trainerId)
                .Select(x => new WorkoutDto(
                    x.Id,
                    x.Name,
                    x.TrainerId))
                .ToListAsync(cancellationToken);
        }
    }
}
