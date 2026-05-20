using FitLead.Domain.Outbox;

namespace FitLead.Application.Common.Outbox
{
    public interface IOutboxMessageHandler
    {
        string Type { get; }

        Task HandleAsync(
            OutboxMessage message,
            CancellationToken cancellationToken);
    }
}
