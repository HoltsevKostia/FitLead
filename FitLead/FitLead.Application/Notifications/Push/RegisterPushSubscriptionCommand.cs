using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Notifications.Push
{
    public sealed record RegisterPushSubscriptionCommand(
        string Endpoint,
        string P256dh,
        string Auth,
        string? UserAgent)
        : IRequest<Result<PushSubscriptionDto>>;
}
