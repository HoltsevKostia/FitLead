using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Users;
using FitLead.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Infrastructure.Repositories
{
    public class TrainerRepository : ITrainerRepository
    {
        private readonly FitLeadDbContext _context;

        public TrainerRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<Trainer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _context.Trainers
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
    }
}
