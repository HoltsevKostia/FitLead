namespace FitLead.Application.Invitations.Queries
{
    public sealed class InvitationDto
    {
        public Guid Id { get; init; }
        public Guid TrainerId { get; init; }
        public string Status { get; init; } = null!;
        public DateTime CreatedAtUtc { get; init; }
        public DateTime ExpiresAtUtc { get; init; }
        public Guid? AcceptedByClientId { get; init; }
        public DateTime? AcceptedAtUtc { get; init; }
    }
}
