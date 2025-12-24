using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Trainings.Queries.TrainingProgram;
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
    }
}
