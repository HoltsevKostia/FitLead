namespace FitLead.Api.Contracts.Invitations
{
    public sealed record AcceptInvitationRequest(
        Guid ClientId
    );
}
