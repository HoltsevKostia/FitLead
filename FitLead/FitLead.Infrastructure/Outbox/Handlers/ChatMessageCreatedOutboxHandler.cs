using System.Text.Json;
using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Messenger.ChatMessages.Outbox;
using FitLead.Application.Messenger.ChatMessages.Realtime;
using FitLead.Domain.Outbox;

namespace FitLead.Infrastructure.Outbox.Handlers
{
    public sealed class ChatMessageCreatedOutboxHandler : IOutboxMessageHandler
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly IChatMessageReadRepository _chatMessageReadRepository;
        private readonly IChatRealtimeNotifier _chatRealtimeNotifier;

        public ChatMessageCreatedOutboxHandler(
            IChatMessageReadRepository chatMessageReadRepository,
            IChatRealtimeNotifier chatRealtimeNotifier)
        {
            _chatMessageReadRepository = chatMessageReadRepository;
            _chatRealtimeNotifier = chatRealtimeNotifier;
        }

        public string Type => OutboxEventTypes.Messenger.ChatMessageCreated;

        public async Task HandleAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Deserialize<ChatMessageCreatedOutboxPayload>(
                message.Payload,
                SerializerOptions);

            if (payload is null)
            {
                throw new InvalidOperationException("Chat message created outbox payload is invalid.");
            }

            var messageDto = await _chatMessageReadRepository.GetMessageAsync(
                payload.MessageId,
                cancellationToken);

            if (messageDto is null)
            {
                throw new InvalidOperationException(
                    $"Chat message '{payload.MessageId}' was not found for outbox message '{message.Id}'.");
            }

            await _chatRealtimeNotifier.MessageCreatedAsync(
                messageDto,
                cancellationToken);
        }
    }
}
