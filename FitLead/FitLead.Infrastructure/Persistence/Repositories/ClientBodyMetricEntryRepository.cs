using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Clients.BodyMetrics;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class ClientBodyMetricEntryRepository : IClientBodyMetricEntryRepository
    {
        private readonly FitLeadDbContext _context;

        public ClientBodyMetricEntryRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            ClientBodyMetricEntry entry,
            CancellationToken cancellationToken)
        {
            await _context.ClientBodyMetricEntries.AddAsync(entry, cancellationToken);
        }

        public async Task<ClientBodyMetricEntry?> GetByIdForClientAsync(
            Guid entryId,
            Guid clientId,
            CancellationToken cancellationToken)
        {
            return await _context.ClientBodyMetricEntries
                .FirstOrDefaultAsync(
                    entry => entry.Id == entryId &&
                             entry.ClientId == clientId,
                    cancellationToken);
        }

        public async Task<bool> ExistsForClientRecordedAtAsync(
            Guid clientId,
            DateOnly recordedAt,
            Guid? excludeEntryId,
            CancellationToken cancellationToken)
        {
            return await _context.ClientBodyMetricEntries
                .AnyAsync(
                    entry => entry.ClientId == clientId &&
                             entry.RecordedAt == recordedAt &&
                             (!excludeEntryId.HasValue || entry.Id != excludeEntryId.Value),
                    cancellationToken);
        }

        public void Remove(ClientBodyMetricEntry entry)
        {
            _context.ClientBodyMetricEntries.Remove(entry);
        }
    }
}
