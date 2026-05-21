using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Notifications.Queries
{
    public sealed record GetNotificationsQuery(
        int? Limit) : IRequest<Result<IReadOnlyList<NotificationDto>>>;
}
