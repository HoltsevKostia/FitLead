using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Common.Results;
using FitLead.Application.Common.Time;
using FitLead.Application.Invitations.Access;
using MediatR;

namespace FitLead.Application.Invitations.Commands
{
    public sealed class DeclineInvitationHandler
    : IRequestHandler<DeclineInvitationCommand, Result>
    {
        private readonly IClock _clock;
        private readonly IInvitationLoader _invitationLoader;
        private readonly IUnitOfWork _unitOfWork;

        public DeclineInvitationHandler(
            IClock clock,
            IInvitationLoader invitationLoader,
            IUnitOfWork unitOfWork)
        {
            _clock = clock;
            _invitationLoader = invitationLoader;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            DeclineInvitationCommand request,
            CancellationToken cancellationToken)
        {
            var invitationResult = await _invitationLoader.GetClientOwnedOrNotFoundAsync(
                request.InvitationId,
                cancellationToken);

            if (invitationResult.IsFailure)
                return Result.Failure(invitationResult.Error);

            var invitation = invitationResult.Value;
            var declineResult = invitation.Decline(_clock.UtcNow);
            if (declineResult.IsFailure)
                return declineResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
