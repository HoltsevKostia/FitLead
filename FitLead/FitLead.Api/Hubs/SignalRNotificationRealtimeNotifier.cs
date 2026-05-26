using FitLead.Application.Notifications.Queries;
using FitLead.Application.Notifications.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace FitLead.Api.Hubs
{
    public sealed class SignalRNotificationRealtimeNotifier : INotificationRealtimeNotifier
    {
        private const string NotificationCreatedEventName = "NotificationCreated";

        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationRealtimeNotifier(
            IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task NotificationCreatedAsync(
            NotificationDto notification,
            CancellationToken cancellationToken)
        {
            return _hubContext.Clients
                .Group(NotificationHubGroups.ForUser(notification.RecipientUserId))
                .SendAsync(
                    NotificationCreatedEventName,
                    notification,
                    cancellationToken);
        }
    }
}
