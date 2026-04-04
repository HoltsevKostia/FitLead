using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Identity;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Invitations;

namespace FitLead.Application.Invitations.Access
{
    public sealed class InvitationLoader : IInvitationLoader
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUserContext _userContext;

        public InvitationLoader(
            IInvitationRepository invitationRepository,
            IUserContext userContext)
        {
            _invitationRepository = invitationRepository;
            _userContext = userContext;
        }

        public async Task<Result<Invitation>> GetClientOwnedOrNotFoundAsync(
            Guid invitationId,
            CancellationToken cancellationToken)
        {
            var currentUserId = _userContext.UserIdOrNull;
            if (!currentUserId.HasValue)
            {
                return Result<Invitation>.Failure(
                    Error.Unauthorized("auth.user_missing", "Current user is missing"));
            }

            var invitation = await _invitationRepository.GetByIdAsync(invitationId, cancellationToken);
            if (invitation is null || invitation.ClientId != currentUserId.Value)
            {
                return Result<Invitation>.Failure(
                    Error.NotFound("invitation.not_found", "Invitation not found"));
            }

            return Result<Invitation>.Success(invitation);
        }
    }
}
