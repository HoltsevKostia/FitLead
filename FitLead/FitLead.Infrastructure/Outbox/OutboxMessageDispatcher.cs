using FitLead.Application.Common.Outbox;
using FitLead.Domain.Outbox;

namespace FitLead.Infrastructure.Outbox
{
    public sealed class OutboxMessageDispatcher : IOutboxMessageDispatcher
    {
        private readonly IReadOnlyDictionary<string, IOutboxMessageHandler> _handlersByType;

        public OutboxMessageDispatcher(IEnumerable<IOutboxMessageHandler> handlers)
        {
            _handlersByType = handlers.ToDictionary(
                handler => handler.Type,
                StringComparer.Ordinal);
        }

        public async Task DispatchAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
        {
            if (!_handlersByType.TryGetValue(message.Type, out var handler))
            {
                throw new UnknownOutboxMessageTypeException(message.Type);
            }

            await handler.HandleAsync(message, cancellationToken);
        }
    }
}
