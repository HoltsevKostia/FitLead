using FitLead.Common.Domain;

namespace FitLead.Domain.Invitations.Events
{
    public sealed record InvitationAcceptedDomainEvent(
        Guid InvitationId,
        Guid TrainerId,
        Guid ClientId
    ) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
