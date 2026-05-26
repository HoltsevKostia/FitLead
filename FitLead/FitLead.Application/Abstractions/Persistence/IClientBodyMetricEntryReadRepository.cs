using FitLead.Application.Clients.BodyMetrics;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IClientBodyMetricEntryReadRepository
    {
        Task<IReadOnlyList<ClientBodyMetricEntryDto>> GetByClientAsync(
            Guid clientId,
            CancellationToken cancellationToken);
    }
}
