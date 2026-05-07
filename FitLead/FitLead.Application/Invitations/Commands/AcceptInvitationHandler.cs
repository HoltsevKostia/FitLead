using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Identity;
using FitLead.Application.Invitations.Services;
using FitLead.Application.Modules.Users;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Invitations.Commands
{
    public sealed class AcceptInvitationHandler
        : IRequestHandler<AcceptInvitationCommand, Result>
    {
        private readonly IUserContext _user;
        private readonly IClock _clock;
        private readonly IUsersModule _usersModule;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IInvitationLinkService _invitationLinkService;

        public AcceptInvitationHandler(
            IUserContext user,
            IClock clock,
            IUsersModule usersModule,
            IUnitOfWork unitOfWork,
            IInvitationRepository invitationRepository,
            IInvitationLinkService invitationLinkService)
        {
            _user = user;
            _clock = clock;
            _usersModule = usersModule;
            _unitOfWork = unitOfWork;
            _invitationRepository = invitationRepository;
            _invitationLinkService = invitationLinkService;
        }

        public async Task<Result> Handle(
            AcceptInvitationCommand request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return Result.Failure(
                    Error.Validation("invitation.accept.token_required", "Token is required"));
            }

            var currentUser = await _usersModule.GetByIdAsync(_user.UserId, cancellationToken);
            if (currentUser is null)
            {
                return Result.Failure(
                    Error.NotFound("client.not_found", "Client not found"));
            }

            if (currentUser.Role != UserRole.Client)
            {
                return Result.Failure(
                    Error.Forbidden("client.required", "User is not a client"));
            }

            var currentUserId = currentUser.Id;
            var tokenHash = _invitationLinkService.ComputeTokenHash(request.Token.Trim());
            var invitation = await _invitationRepository.GetByTokenHashAsync(
                tokenHash,
                cancellationToken);

            if (invitation is null)
            {
                return Result.Failure(
                    Error.NotFound("invitation.not_found", "Invitation not found"));
            }

            var activeTrainerId = await _usersModule.GetActiveTrainerIdForClientAsync(
                currentUserId,
                cancellationToken);

            if (activeTrainerId.HasValue && activeTrainerId.Value != invitation.TrainerId)
            {
                return Result.Failure(
                    Error.Conflict(
                        "invitation.accept.client_has_another_trainer",
                        "Client already has another active trainer"));
            }

            var acceptResult = invitation.Accept(currentUserId, _clock.UtcNow);
            if (acceptResult.IsFailure)
            {
                return acceptResult;
            }

            await _usersModule.EnsureTrainerClientRelationshipAsync(
                invitation.TrainerId,
                currentUserId,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
