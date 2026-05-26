using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Notifications.Commands
{
    public sealed record MarkNotificationReadCommand(
        Guid NotificationId) : IRequest<Result>;
}
