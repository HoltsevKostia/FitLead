using MediatR;

namespace FitLead.Application.Invitations.EventHandlers
{
    public sealed record InvitationAcceptedNotification(
        Guid InvitationId,
        Guid TrainerId,
        Guid ClientId
    ) : INotification;
}
