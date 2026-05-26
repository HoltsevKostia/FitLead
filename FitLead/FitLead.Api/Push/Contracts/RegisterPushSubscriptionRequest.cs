namespace FitLead.Api.Push.Contracts
{
    public sealed record RegisterPushSubscriptionRequest(
        string Endpoint,
        PushSubscriptionKeysRequest? Keys,
        string? UserAgent);
}
