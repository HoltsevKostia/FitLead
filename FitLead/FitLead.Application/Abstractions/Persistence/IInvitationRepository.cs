using FitLead.Domain.Invitations;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IInvitationRepository
    {
        Task AddAsync(
            Invitation invitation,
            CancellationToken cancellationToken);

        Task<Invitation?> GetByIdAsync(
            Guid invitationId,
            CancellationToken cancellationToken);

        Task<Invitation?> GetByTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken);
    }
}
