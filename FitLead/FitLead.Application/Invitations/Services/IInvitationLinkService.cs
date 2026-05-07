namespace FitLead.Application.Invitations.Services
{
    public interface IInvitationLinkService
    {
        InvitationLinkPayload CreateLink();
        string ComputeTokenHash(string token);
    }
}
