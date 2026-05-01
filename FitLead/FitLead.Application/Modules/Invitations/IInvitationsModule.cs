namespace FitLead.Application.Modules.Invitations
{
    public interface IInvitationsModule
    {
        Task<InvitationModuleDescriptor?> GetByIdAsync(
            Guid invitationId,
            CancellationToken cancellationToken = default);

        Task<bool> HasPendingInvitationAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken = default);
    }
}
