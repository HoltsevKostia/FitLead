using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Clients.BodyMetrics;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class ClientBodyMetricEntryReadRepository : IClientBodyMetricEntryReadRepository
    {
        private readonly FitLeadDbContext _context;

        public ClientBodyMetricEntryReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<ClientBodyMetricEntryDto>> GetByClientAsync(
            Guid clientId,
            CancellationToken cancellationToken)
        {
            return await _context.ClientBodyMetricEntries
                .AsNoTracking()
                .Where(entry => entry.ClientId == clientId)
                .OrderByDescending(entry => entry.RecordedAt)
                .ThenByDescending(entry => entry.Id)
                .Select(entry => new ClientBodyMetricEntryDto(
                    entry.Id,
                    entry.ClientId,
                    entry.RecordedAt,
                    entry.WeightKg,
                    entry.BodyFatPercent,
                    entry.ChestCm,
                    entry.WaistCm,
                    entry.HipsCm,
                    entry.ArmCm,
                    entry.ThighCm,
                    entry.Note,
                    entry.CreatedAtUtc,
                    entry.UpdatedAtUtc))
                .ToListAsync(cancellationToken);
        }
    }
}
