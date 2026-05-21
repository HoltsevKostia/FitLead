namespace FitLead.Application.Notifications.Outbox
{
    public sealed record NotificationCreatedOutboxPayload(
        Guid NotificationId,
        Guid RecipientUserId,
        DateTime CreatedAtUtc);
}
