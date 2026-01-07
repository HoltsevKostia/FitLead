using FitLead.Domain.Common;


namespace FitLead.Domain.Invitations.Events
{
    public sealed record InvitationExpiredDomainEvent(
        Guid InvitationId,
        Guid TrainerId,
        Guid ClientId
    ) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
