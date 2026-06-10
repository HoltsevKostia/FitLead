using FitLead.Application.ClientDashboard.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IClientDashboardReadRepository
    {
        Task<ClientDashboardDto> GetAsync(
            Guid clientId,
            DateTime utcNow,
            CancellationToken cancellationToken);
    }
}
