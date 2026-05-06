using FitLead.Application.Invitations.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IInvitationReadRepository
    {
        Task<IReadOnlyList<InvitationDto>> GetSentByTrainerAsync(
            Guid trainerId,
            CancellationToken cancellationToken);
    }
}
