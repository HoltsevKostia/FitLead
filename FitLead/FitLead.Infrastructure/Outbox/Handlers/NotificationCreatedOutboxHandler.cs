using System.Text.Json;
using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Common.Time;
using FitLead.Application.Notifications.Outbox;
using FitLead.Application.Notifications.Push;
using FitLead.Application.Notifications.Realtime;
using FitLead.Domain.Outbox;
using FitLead.Infrastructure.Notifications.Push;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitLead.Infrastructure.Outbox.Handlers
{
    public sealed class NotificationCreatedOutboxHandler : IOutboxMessageHandler
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly INotificationReadRepository _notificationReadRepository;
        private readonly INotificationRealtimeNotifier _notificationRealtimeNotifier;
        private readonly IPushSubscriptionRepository _pushSubscriptionRepository;
        private readonly IWebPushSender _webPushSender;
        private readonly IClock _clock;
        private readonly PushOptions _pushOptions;
        private readonly ILogger<NotificationCreatedOutboxHandler> _logger;

        public NotificationCreatedOutboxHandler(
            INotificationReadRepository notificationReadRepository,
            INotificationRealtimeNotifier notificationRealtimeNotifier,
            IPushSubscriptionRepository pushSubscriptionRepository,
            IWebPushSender webPushSender,
            IClock clock,
            IOptions<PushOptions> pushOptions,
            ILogger<NotificationCreatedOutboxHandler> logger)
        {
            _notificationReadRepository = notificationReadRepository;
            _notificationRealtimeNotifier = notificationRealtimeNotifier;
            _pushSubscriptionRepository = pushSubscriptionRepository;
            _webPushSender = webPushSender;
            _clock = clock;
            _pushOptions = pushOptions.Value;
            _logger = logger;
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

            if (!_pushOptions.Enabled)
            {
                _logger.LogInformation(
                    "Web Push delivery is disabled for notification {NotificationId}",
                    notification.Id);

                return;
            }

            var subscriptions = await _pushSubscriptionRepository.GetActiveByUserIdAsync(
                notification.RecipientUserId,
                cancellationToken);

            if (subscriptions.Count == 0)
            {
                _logger.LogInformation(
                    "No active push subscriptions found for notification {NotificationId} recipient {RecipientUserId}",
                    notification.Id,
                    notification.RecipientUserId);

                return;
            }

            _logger.LogInformation(
                "Sending web push for notification {NotificationId} to {SubscriptionCount} active subscription(s)",
                notification.Id,
                subscriptions.Count);

            var pushNotification = new WebPushNotification(
                "Нове сповіщення у FitLead",
                "Відкрийте FitLead, щоб переглянути деталі.",
                notification.LinkUrl);

            foreach (var subscription in subscriptions)
            {
                try
                {
                    var result = await _webPushSender.SendAsync(
                        subscription,
                        pushNotification,
                        cancellationToken);

                    var now = _clock.UtcNow;
                    var isExpired = result == WebPushSendResult.SubscriptionExpired;
                    var stateResult = isExpired
                        ? subscription.Revoke(now)
                        : subscription.MarkUsed(now);

                    if (stateResult.IsFailure)
                    {
                        _logger.LogWarning(
                            "Could not update push subscription {PushSubscriptionId} after notification {NotificationId}: {ErrorCode}",
                            subscription.Id,
                            notification.Id,
                            stateResult.Error.Code);
                    }

                    if (isExpired)
                    {
                        _logger.LogInformation(
                            "Revoked expired push subscription {PushSubscriptionId} after notification {NotificationId}",
                            subscription.Id,
                            notification.Id);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Web push sent for notification {NotificationId} to subscription {PushSubscriptionId}",
                            notification.Id,
                            subscription.Id);
                    }
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogWarning(
                        exception,
                        "Could not send web push for notification {NotificationId} to subscription {PushSubscriptionId}",
                        notification.Id,
                        subscription.Id);
                }
            }
        }
    }
}
