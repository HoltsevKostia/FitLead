using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Trainings;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public class TrainingProgramRepository : ITrainingProgramRepository
    {
        private readonly FitLeadDbContext _context;

        public TrainingProgramRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<TrainingProgram?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
        {
            return await _context.TrainingPrograms
                .Include(x => x.Workouts)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task AddAsync(
            TrainingProgram program,
            CancellationToken cancellationToken)
        {
            await _context.TrainingPrograms.AddAsync(program, cancellationToken);
        }

        public async Task DeleteTrainingProgramWorkoutsByProgramIdAsync(
            Guid programId,
            CancellationToken cancellationToken)
        {
            await _context.TrainingProgramWorkouts
                .Where(x => x.TrainingProgramId == programId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public void Remove(TrainingProgram program)
        {
            _context.TrainingPrograms.Remove(program);
        }
    }
}
