using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Trainings;
using FitLead.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public class TrainingProgramRepository : ITrainingProgramRepository
    {
        private readonly FitLeadDbContext _context;

        public TrainingProgramRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            TrainingProgram program,
            CancellationToken cancellationToken)
        {
            await _context.TrainingPrograms.AddAsync(program, cancellationToken);
        }
    }
}
