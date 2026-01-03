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

        Task<bool> ExistsPendingAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken);

        Task<int> CountSentByTrainerForDateAsync(
            Guid trainerId,
            DateTime dateUtc,
            CancellationToken cancellationToken);
    }
}
