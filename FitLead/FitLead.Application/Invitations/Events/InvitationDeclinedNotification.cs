using MediatR;

namespace FitLead.Application.Invitations.Events
{
    public sealed record InvitationDeclinedNotification(
        Guid InvitationId,
        Guid TrainerId,
        Guid ClientId
    ) : INotification;
}
