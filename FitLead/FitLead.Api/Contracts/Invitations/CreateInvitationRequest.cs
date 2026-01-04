namespace FitLead.Api.Contracts.Invitations
{
    public sealed record CreateInvitationRequest(
        Guid TrainerId,
        Guid ClientId
    );
}
