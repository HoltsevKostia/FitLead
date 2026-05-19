using FitLead.Domain.Outbox;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IOutboxMessageRepository
    {
        Task AddAsync(
            OutboxMessage outboxMessage,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
            DateTime utcNow,
            int batchSize,
            CancellationToken cancellationToken);
    }
}
