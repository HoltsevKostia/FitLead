namespace FitLead.Api.Push.Contracts
{
    public sealed record PushSubscriptionKeysRequest(
        string P256dh,
        string Auth);
}
