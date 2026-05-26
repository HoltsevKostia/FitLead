using FitLead.Domain.Clients.ClientProfiles;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IClientProfileRepository
    {
        Task AddAsync(ClientProfile clientProfile, CancellationToken cancellationToken);

        Task<ClientProfile?> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken);
    }
}
