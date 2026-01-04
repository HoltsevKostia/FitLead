namespace FitLead.Api.Contracts.Invitations
{
    public sealed record DeclineInvitationRequest(
        Guid ClientId
    );
}
