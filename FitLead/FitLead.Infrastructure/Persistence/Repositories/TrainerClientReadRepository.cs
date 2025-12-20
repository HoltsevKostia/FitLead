using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Queries;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class TrainerClientReadRepository
    : ITrainerClientReadRepository
    {
        private readonly FitLeadDbContext _context;

        public TrainerClientReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TrainerClientDto>> GetClientsByTrainerIdAsync(
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            return await (
                from tc in _context.TrainerClients
                join u in _context.Users on tc.ClientId equals u.Id
                where tc.TrainerId == trainerId
                select new TrainerClientDto
                {
                    ClientId = u.Id,
                    Email = u.Email,
                    FullName = u.FullName
                }
            ).ToListAsync(cancellationToken);
        }
    }
}
