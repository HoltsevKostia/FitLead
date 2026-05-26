using System.Text.Json;
using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Outbox;
using FitLead.Domain.Outbox;

namespace FitLead.Infrastructure.Outbox
{
    public sealed class Outbox : IOutbox
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly IOutboxMessageRepository _outboxMessageRepository;

        public Outbox(IOutboxMessageRepository outboxMessageRepository)
        {
            _outboxMessageRepository = outboxMessageRepository;
        }

        public async Task EnqueueAsync<TPayload>(
            string type,
            TPayload payload,
            DateTime occurredAtUtc,
            CancellationToken cancellationToken)
        {
            var serializedPayload = JsonSerializer.Serialize(payload, SerializerOptions);
            JsonDocument.Parse(serializedPayload);

            var createResult = OutboxMessage.Create(
                type,
                string.IsNullOrWhiteSpace(serializedPayload) ? "{}" : serializedPayload,
                occurredAtUtc);

            if (createResult.IsFailure)
            {
                throw new InvalidOperationException(createResult.Error.Message);
            }

            await _outboxMessageRepository.AddAsync(createResult.Value, cancellationToken);
        }
    }
}
