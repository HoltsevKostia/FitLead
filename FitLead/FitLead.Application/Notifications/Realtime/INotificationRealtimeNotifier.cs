using FitLead.Application.Notifications.Queries;

namespace FitLead.Application.Notifications.Realtime
{
    public interface INotificationRealtimeNotifier
    {
        Task NotificationCreatedAsync(
            NotificationDto notification,
            CancellationToken cancellationToken);
    }
}
