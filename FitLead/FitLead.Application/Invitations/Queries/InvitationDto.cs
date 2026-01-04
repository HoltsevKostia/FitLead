using FitLead.Domain.Invitations;


namespace FitLead.Application.Invitations.Queries
{
    public sealed class InvitationDto
    {
        public Guid Id { get; init; }
        public Guid TrainerId { get; init; }
        public Guid ClientId { get; init; }
        public InvitationStatus Status { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime ExpiresAt { get; init; }
    }
}
