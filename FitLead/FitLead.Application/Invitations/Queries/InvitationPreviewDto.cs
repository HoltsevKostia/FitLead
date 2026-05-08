namespace FitLead.Application.Invitations.Queries
{
    public sealed class InvitationPreviewDto
    {
        public string Status { get; init; } = null!;
        public bool IsJoinable { get; init; }
        public DateTime ExpiresAtUtc { get; init; }
        public InvitationTrainerPreviewDto Trainer { get; init; } = null!;
    }
}
