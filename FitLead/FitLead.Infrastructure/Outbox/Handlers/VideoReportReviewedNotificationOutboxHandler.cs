using System.Text.Json;
using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Messenger.VideoReports.Outbox;
using FitLead.Application.Notifications.Outbox;
using FitLead.Domain.Notifications;
using FitLead.Domain.Outbox;

namespace FitLead.Infrastructure.Outbox.Handlers
{
    public sealed class VideoReportReviewedNotificationOutboxHandler : IOutboxMessageHandler
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly INotificationRepository _notificationRepository;
        private readonly IOutbox _outbox;

        public VideoReportReviewedNotificationOutboxHandler(
            INotificationRepository notificationRepository,
            IOutbox outbox)
        {
            _notificationRepository = notificationRepository;
            _outbox = outbox;
        }

        public string Type => OutboxEventTypes.Messenger.VideoReportReviewed;

        public async Task HandleAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Deserialize<VideoReportReviewedOutboxPayload>(
                message.Payload,
                SerializerOptions);

            if (payload is null)
            {
                throw new InvalidOperationException("Video report reviewed outbox payload is invalid.");
            }

            var existingNotification = await _notificationRepository.GetBySourceEventAsync(
                message.Id,
                payload.ClientId,
                NotificationType.VideoReportReviewed,
                cancellationToken);
            if (existingNotification is not null)
            {
                return;
            }

            var notificationResult = Notification.Create(
                payload.ClientId,
                NotificationType.VideoReportReviewed,
                "Відео-звіт переглянуто",
                payload.Title,
                $"/chats/{payload.ChatId}/reports/{payload.ReportId}",
                payload.ReviewedAtUtc,
                message.Id);
            if (notificationResult.IsFailure)
            {
                throw new InvalidOperationException(notificationResult.Error.Message);
            }

            var notification = notificationResult.Value;
            await _notificationRepository.AddAsync(notification, cancellationToken);
            await _outbox.EnqueueAsync(
                OutboxEventTypes.Notifications.Created,
                new NotificationCreatedOutboxPayload(
                    notification.Id,
                    notification.RecipientUserId,
                    notification.CreatedAtUtc),
                notification.CreatedAtUtc,
                cancellationToken);
        }
    }
}
