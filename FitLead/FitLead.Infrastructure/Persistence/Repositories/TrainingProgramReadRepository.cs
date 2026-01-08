using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Trainings.Queries.TrainingProgram;
using FitLead.Application.Trainings.Queries.Workout;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class TrainingProgramReadRepository
    : ITrainingProgramReadRepository
    {
        private readonly FitLeadDbContext _context;

        public TrainingProgramReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TrainingProgramDto>> GetByTrainerIdAsync(
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            return await _context.TrainingPrograms
                .Where(x => x.TrainerId == trainerId)
                .Select(x => new TrainingProgramDto
                {
                    Id = x.Id,
                    Title = x.Title
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<WorkoutDto>> GetWorkoutsByProgramIdAsync(Guid programId, CancellationToken cancellationToken)
        {
            return await (
                from tpw in _context.TrainingProgramWorkouts
                join w in _context.Workouts
                    on tpw.WorkoutId equals w.Id
                where EF.Property<Guid>(tpw, "TrainingProgramId") == programId
                orderby tpw.Order
                select new WorkoutDto(
                    w.Id,
                    w.Name,
                    w.TrainerId
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
