using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Errors;
using FitLead.Application.Common.Identity;
using FitLead.Application.Common.Results;
using FitLead.Application.Common.Time;
using MediatR;

namespace FitLead.Application.Invitations.Commands
{
    public sealed class DeclineInvitationHandler
    : IRequestHandler<DeclineInvitationCommand, Result>
    {
        private readonly IUserContext _user;
        private readonly IClock _clock;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeclineInvitationHandler(
            IUserContext user,
            IClock clock,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork)
        {
            _user = user;
            _clock = clock;
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            DeclineInvitationCommand request,
            CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(
                request.InvitationId,
                cancellationToken);

            if (invitation is null)
                return Result.Failure(Error.NotFound("invitation.not_found", "Invitation not found"));

            if (invitation.ClientId != _user.UserId)
                return Result.Failure(Error.Forbidden("invitation.forbidden", "Invitation does not belong to this client"));

            invitation.Decline(_clock.UtcNow);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
