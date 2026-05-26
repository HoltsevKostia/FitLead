using FitLead.Application.Notifications.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface INotificationReadRepository
    {
        Task<IReadOnlyList<NotificationDto>> GetByRecipientAsync(
            Guid recipientUserId,
            int limit,
            CancellationToken cancellationToken);

        Task<NotificationDto?> GetDetailsByIdForRecipientAsync(
            Guid notificationId,
            Guid recipientUserId,
            CancellationToken cancellationToken);

        Task<int> GetUnreadCountAsync(
            Guid recipientUserId,
            CancellationToken cancellationToken);
    }
}
