using FitLead.Domain.Invitations;

namespace FitLead.Application.Invitations.Queries
{
    public sealed class InvitationDto
    {
        public Guid Id { get; init; }
        public Guid TrainerId { get; init; }
        public InvitationStatus Status { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime ExpiresAtUtc { get; init; }
        public Guid? AcceptedByClientId { get; init; }
        public DateTime? AcceptedAtUtc { get; init; }
    }
}
