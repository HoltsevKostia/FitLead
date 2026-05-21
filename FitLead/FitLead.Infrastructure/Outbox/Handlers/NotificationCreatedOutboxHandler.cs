using System.Text.Json;
using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Notifications.Outbox;
using FitLead.Application.Notifications.Realtime;
using FitLead.Domain.Outbox;

namespace FitLead.Infrastructure.Outbox.Handlers
{
    public sealed class NotificationCreatedOutboxHandler : IOutboxMessageHandler
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly INotificationReadRepository _notificationReadRepository;
        private readonly INotificationRealtimeNotifier _notificationRealtimeNotifier;

        public NotificationCreatedOutboxHandler(
            INotificationReadRepository notificationReadRepository,
            INotificationRealtimeNotifier notificationRealtimeNotifier)
        {
            _notificationReadRepository = notificationReadRepository;
            _notificationRealtimeNotifier = notificationRealtimeNotifier;
        }

        public string Type => OutboxEventTypes.Notifications.Created;

        public async Task HandleAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Deserialize<NotificationCreatedOutboxPayload>(
                message.Payload,
                SerializerOptions);

            if (payload is null)
            {
                throw new InvalidOperationException("Notification created outbox payload is invalid.");
            }

            var notification = await _notificationReadRepository.GetDetailsByIdForRecipientAsync(
                payload.NotificationId,
                payload.RecipientUserId,
                cancellationToken);

            if (notification is null)
            {
                throw new InvalidOperationException(
                    $"Notification '{payload.NotificationId}' was not found for outbox message '{message.Id}'.");
            }

            await _notificationRealtimeNotifier.NotificationCreatedAsync(
                notification,
                cancellationToken);
        }
    }
}
