using FitLead.Application.Invitations.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IInvitationReadRepository
    {
        Task<IReadOnlyList<InvitationDto>> GetPendingForClientAsync(
            Guid clientId,
            DateTime now,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<InvitationDto>> GetSentByTrainerAsync(
            Guid trainerId,
            CancellationToken cancellationToken);
    }
}
