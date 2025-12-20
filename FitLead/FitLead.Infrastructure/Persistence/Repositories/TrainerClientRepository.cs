using FitLead.Application.Abstractions.Persistence;
using FitLead.Infrastructure.Persistence;
using FitLead.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class TrainerClientRepository : ITrainerClientRepository
    {
        private readonly FitLeadDbContext _context;

        public TrainerClientRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken)
        {
            return await _context.TrainerClients
                .AnyAsync(
                    x => x.TrainerId == trainerId &&
                         x.ClientId == clientId,
                    cancellationToken);
        }

        public async Task AddAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var entity = new TrainerClient(trainerId, clientId);
            await _context.TrainerClients.AddAsync(entity, cancellationToken);
        }
    }
}
