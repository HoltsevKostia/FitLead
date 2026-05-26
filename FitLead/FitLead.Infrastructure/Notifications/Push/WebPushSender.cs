using System.Net;
using System.Text.Json;
using FitLead.Application.Notifications.Push;
using Microsoft.Extensions.Options;
using WebPush;
using DomainPushSubscription = FitLead.Domain.Notifications.PushSubscriptions.PushSubscription;

namespace FitLead.Infrastructure.Notifications.Push
{
    public sealed class WebPushSender : IWebPushSender
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly PushOptions _options;

        public WebPushSender(IOptions<PushOptions> options)
        {
            _options = options.Value;
        }

        public async Task<WebPushSendResult> SendAsync(
            DomainPushSubscription subscription,
            WebPushNotification notification,
            CancellationToken cancellationToken)
        {
            EnsureConfigured();

            var webPushSubscription = new WebPush.PushSubscription(
                subscription.Endpoint,
                subscription.P256dh,
                subscription.Auth);

            var vapidDetails = new VapidDetails(
                _options.Subject,
                _options.VapidPublicKey,
                _options.VapidPrivateKey);

            var payload = JsonSerializer.Serialize(notification, SerializerOptions);

            using var client = new WebPushClient();

            try
            {
                await client.SendNotificationAsync(
                    webPushSubscription,
                    payload,
                    vapidDetails,
                    cancellationToken: cancellationToken);

                return WebPushSendResult.Sent;
            }
            catch (WebPushException exception) when (IsExpiredSubscription(exception))
            {
                return WebPushSendResult.SubscriptionExpired;
            }
        }

        private void EnsureConfigured()
        {
            if (string.IsNullOrWhiteSpace(_options.Subject) ||
                string.IsNullOrWhiteSpace(_options.VapidPublicKey) ||
                string.IsNullOrWhiteSpace(_options.VapidPrivateKey))
            {
                throw new InvalidOperationException("Web Push VAPID configuration is missing.");
            }
        }

        private static bool IsExpiredSubscription(WebPushException exception)
        {
            return exception.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone;
        }
    }
}
