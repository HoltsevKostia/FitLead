using FitLead.Domain.Outbox;

namespace FitLead.Application.Common.Outbox
{
    public interface IOutboxMessageDispatcher
    {
        Task DispatchAsync(
            OutboxMessage message,
            CancellationToken cancellationToken);
    }
}
