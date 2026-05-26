using FitLead.Domain.Clients.BodyMetrics;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IClientBodyMetricEntryRepository
    {
        Task AddAsync(ClientBodyMetricEntry entry, CancellationToken cancellationToken);

        Task<ClientBodyMetricEntry?> GetByIdForClientAsync(
            Guid entryId,
            Guid clientId,
            CancellationToken cancellationToken);

        Task<bool> ExistsForClientRecordedAtAsync(
            Guid clientId,
            DateOnly recordedAt,
            Guid? excludeEntryId,
            CancellationToken cancellationToken);

        void Remove(ClientBodyMetricEntry entry);
    }
}
