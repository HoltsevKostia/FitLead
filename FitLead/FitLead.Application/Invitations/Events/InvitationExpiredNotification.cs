using MediatR;

namespace FitLead.Application.Invitations.Events
{
    public sealed record InvitationExpiredNotification(
        Guid InvitationId,
        Guid TrainerId,
        Guid ClientId
    ) : INotification;
}
