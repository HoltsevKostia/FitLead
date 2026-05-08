namespace FitLead.Application.Invitations.Services
{
    public sealed record InvitationLinkPayload(
        string Token,
        string TokenHash,
        string InviteUrl);
}
