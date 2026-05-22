using FitLead.Domain.Notifications.PushSubscriptions;

namespace FitLead.Application.Notifications.Push
{
    public interface IWebPushSender
    {
        Task<WebPushSendResult> SendAsync(
            PushSubscription subscription,
            WebPushNotification notification,
            CancellationToken cancellationToken);
    }
}
