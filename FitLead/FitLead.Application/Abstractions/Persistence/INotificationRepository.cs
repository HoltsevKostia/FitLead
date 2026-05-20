using FitLead.Domain.Notifications;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface INotificationRepository
    {
        Task AddAsync(
            Notification notification,
            CancellationToken cancellationToken);

        Task<Notification?> GetByIdForRecipientAsync(
            Guid notificationId,
            Guid recipientUserId,
            CancellationToken cancellationToken);

        Task<Notification?> GetBySourceEventAsync(
            Guid sourceEventId,
            Guid recipientUserId,
            NotificationType type,
            CancellationToken cancellationToken);
    }
}
