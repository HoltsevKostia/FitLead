using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Notifications.Queries
{
    public sealed record GetUnreadNotificationCountQuery
        : IRequest<Result<UnreadNotificationCountDto>>;
}
