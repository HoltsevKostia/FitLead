using System.Text.Json;
using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Outbox;
using FitLead.Application.Notifications.Outbox;
using FitLead.Application.Trainings.TrainingProgramAssignments.Outbox;
using FitLead.Domain.Notifications;
using FitLead.Domain.Outbox;

namespace FitLead.Infrastructure.Outbox.Handlers
{
    public sealed class TrainingProgramAssignedNotificationOutboxHandler : IOutboxMessageHandler
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly INotificationRepository _notificationRepository;
        private readonly IOutbox _outbox;

        public TrainingProgramAssignedNotificationOutboxHandler(
            INotificationRepository notificationRepository,
            IOutbox outbox)
        {
            _notificationRepository = notificationRepository;
            _outbox = outbox;
        }

        public string Type => OutboxEventTypes.Training.ProgramAssigned;

        public async Task HandleAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Deserialize<TrainingProgramAssignedOutboxPayload>(
                message.Payload,
                SerializerOptions);

            if (payload is null)
            {
                throw new InvalidOperationException("Training program assigned outbox payload is invalid.");
            }

            var existingNotification = await _notificationRepository.GetBySourceEventAsync(
                message.Id,
                payload.ClientId,
                NotificationType.TrainingProgramAssigned,
                cancellationToken);
            if (existingNotification is not null)
            {
                return;
            }

            var notificationResult = Notification.Create(
                payload.ClientId,
                NotificationType.TrainingProgramAssigned,
                "Призначено програму тренувань",
                payload.ProgramTitle,
                $"/client/training-programs/{payload.AssignmentId}",
                payload.AssignedAtUtc,
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
