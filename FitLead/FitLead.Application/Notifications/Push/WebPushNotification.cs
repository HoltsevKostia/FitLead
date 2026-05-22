namespace FitLead.Application.Notifications.Push
{
    public sealed record WebPushNotification(
        string Title,
        string Body,
        string Url);
}
