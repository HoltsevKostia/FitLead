using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Notifications.Commands
{
    public sealed record MarkAllNotificationsReadCommand : IRequest<Result>;
}
