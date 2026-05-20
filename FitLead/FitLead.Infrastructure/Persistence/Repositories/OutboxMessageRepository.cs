using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Outbox;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class OutboxMessageRepository : IOutboxMessageRepository
    {
        private readonly FitLeadDbContext _context;

        public OutboxMessageRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            OutboxMessage outboxMessage,
            CancellationToken cancellationToken)
        {
            await _context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
        }

        public async Task<OutboxMessage?> GetByIdAsync(
            Guid outboxMessageId,
            CancellationToken cancellationToken)
        {
            return await _context.OutboxMessages
                .FirstOrDefaultAsync(
                    outboxMessage => outboxMessage.Id == outboxMessageId,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
            DateTime utcNow,
            int batchSize,
            CancellationToken cancellationToken)
        {
            if (batchSize <= 0)
            {
                return Array.Empty<OutboxMessage>();
            }

            return await _context.OutboxMessages
                .Where(outboxMessage =>
                    outboxMessage.Status == OutboxMessageStatus.Pending &&
                    (outboxMessage.NextRetryAtUtc == null ||
                     outboxMessage.NextRetryAtUtc <= utcNow))
                .OrderBy(outboxMessage => outboxMessage.OccurredAtUtc)
                .ThenBy(outboxMessage => outboxMessage.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }
    }
}
