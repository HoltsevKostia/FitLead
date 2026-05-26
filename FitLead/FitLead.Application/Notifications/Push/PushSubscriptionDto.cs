namespace FitLead.Application.Notifications.Push
{
    public sealed record PushSubscriptionDto(
        Guid Id,
        string Endpoint,
        DateTime CreatedAtUtc);
}
