namespace FitLead.Api.Invitations.Contracts
{
    public sealed record CreateInvitationRequest(
        int ExpiresInDays
    );
}
