using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Clients.ClientProfiles;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class ClientProfileRepository : IClientProfileRepository
    {
        private readonly FitLeadDbContext _context;

        public ClientProfileRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ClientProfile clientProfile, CancellationToken cancellationToken)
        {
            await _context.ClientProfiles.AddAsync(clientProfile, cancellationToken);
        }

        public async Task<ClientProfile?> GetByClientIdAsync(
            Guid clientId,
            CancellationToken cancellationToken)
        {
            return await _context.ClientProfiles
                .FirstOrDefaultAsync(x => x.ClientId == clientId, cancellationToken);
        }
    }
}
