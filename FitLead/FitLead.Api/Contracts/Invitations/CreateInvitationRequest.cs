namespace FitLead.Api.Contracts.Invitations
{
    public sealed record CreateInvitationRequest(
        Guid ClientId
    );
}
