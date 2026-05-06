namespace FitLead.Application.Invitations.Commands
{
    public sealed record CreateInvitationResult(
        Guid InvitationId,
        string InviteUrl,
        DateTime ExpiresAtUtc);
}
