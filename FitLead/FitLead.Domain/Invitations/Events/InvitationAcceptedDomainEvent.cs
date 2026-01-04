using MediatR;


namespace FitLead.Domain.Invitations.Events
{
    public sealed record InvitationAcceptedDomainEvent(
        Guid InvitationId,
        Guid TrainerId,
        Guid ClientId
    ) : INotification;
}
