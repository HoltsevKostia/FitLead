using FitLead.Common.Results;
using FitLead.Domain.Invitations;

namespace FitLead.Application.Invitations.Access
{
    public interface IInvitationLoader
    {
        Task<Result<Invitation>> GetClientOwnedOrNotFoundAsync(
            Guid invitationId,
            CancellationToken cancellationToken);
    }
}
