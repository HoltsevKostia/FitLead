using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Identity;
using FitLead.Application.Modules.Users;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Invitations.Commands
{
    public sealed class RevokeInvitationHandler
        : IRequestHandler<RevokeInvitationCommand, Result>
    {
        private readonly IUserContext _user;
        private readonly IClock _clock;
        private readonly IUsersModule _usersModule;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInvitationRepository _invitationRepository;

        public RevokeInvitationHandler(
            IUserContext user,
            IClock clock,
            IUsersModule usersModule,
            IUnitOfWork unitOfWork,
            IInvitationRepository invitationRepository)
        {
            _user = user;
            _clock = clock;
            _usersModule = usersModule;
            _unitOfWork = unitOfWork;
            _invitationRepository = invitationRepository;
        }

        public async Task<Result> Handle(
            RevokeInvitationCommand request,
            CancellationToken cancellationToken)
        {
            var currentUser = await _usersModule.GetByIdAsync(_user.UserId, cancellationToken);
            if (currentUser is null)
            {
                return Result.Failure(
                    Error.NotFound("trainer.not_found", "Trainer not found"));
            }

            if (currentUser.Role != UserRole.Trainer)
            {
                return Result.Failure(
                    Error.Forbidden("trainer.required", "User is not a trainer"));
            }

            var invitation = await _invitationRepository.GetByIdAsync(
                request.InvitationId,
                cancellationToken);

            if (invitation is null || invitation.TrainerId != currentUser.Id)
            {
                return Result.Failure(
                    Error.NotFound("invitation.not_found", "Invitation not found"));
            }

            var revokeResult = invitation.Revoke(_clock.UtcNow);
            if (revokeResult.IsFailure)
            {
                return revokeResult;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
