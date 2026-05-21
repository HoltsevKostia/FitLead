using System.Text.Json;
using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Messenger.VideoReports.Outbox;
using FitLead.Domain.Notifications;
using FitLead.Domain.Outbox;

namespace FitLead.Infrastructure.Outbox.Handlers
{
    public sealed class VideoReportSubmittedNotificationOutboxHandler : IOutboxMessageHandler
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly INotificationRepository _notificationRepository;

        public VideoReportSubmittedNotificationOutboxHandler(
            INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public string Type => OutboxEventTypes.Messenger.VideoReportSubmitted;

        public async Task HandleAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Deserialize<VideoReportSubmittedOutboxPayload>(
                message.Payload,
                SerializerOptions);

            if (payload is null)
            {
                throw new InvalidOperationException("Video report submitted outbox payload is invalid.");
            }

            var existingNotification = await _notificationRepository.GetBySourceEventAsync(
                message.Id,
                payload.TrainerId,
                NotificationType.VideoReportSubmitted,
                cancellationToken);
            if (existingNotification is not null)
            {
                return;
            }

            var notificationResult = Notification.Create(
                payload.TrainerId,
                NotificationType.VideoReportSubmitted,
                "Новий відео-звіт",
                payload.Title,
                $"/chats/{payload.ChatId}/reports/{payload.ReportId}",
                payload.SubmittedAtUtc,
                message.Id);
            if (notificationResult.IsFailure)
            {
                throw new InvalidOperationException(notificationResult.Error.Message);
            }

            await _notificationRepository.AddAsync(notificationResult.Value, cancellationToken);
        }
    }
}
