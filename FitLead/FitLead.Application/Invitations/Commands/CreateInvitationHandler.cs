using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Identity;
using FitLead.Application.Invitations.Services;
using FitLead.Application.Modules.Users;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Invitations;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Invitations.Commands
{
    public sealed class CreateInvitationHandler
        : IRequestHandler<CreateInvitationCommand, Result<CreateInvitationResult>>
    {
        private static readonly HashSet<int> AllowedExpiryDays = [7, 14];

        private readonly IUserContext _user;
        private readonly IClock _clock;
        private readonly IUsersModule _usersModule;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IInvitationLinkService _invitationLinkService;

        public CreateInvitationHandler(
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

        public async Task<Result<CreateInvitationResult>> Handle(
            CreateInvitationCommand request,
            CancellationToken cancellationToken)
        {
            var trainer = await _usersModule.GetByIdAsync(_user.UserId, cancellationToken);
            if (trainer is null)
            {
                return Result<CreateInvitationResult>.Failure(
                    Error.NotFound("trainer.not_found", "Trainer not found"));
            }

            if (trainer.Role != UserRole.Trainer)
            {
                return Result<CreateInvitationResult>.Failure(
                    Error.Forbidden("trainer.required", "User is not a trainer"));
            }

            if (!AllowedExpiryDays.Contains(request.ExpiresInDays))
            {
                return Result<CreateInvitationResult>.Failure(
                    Error.Validation("invitation.create.expires_in_days_invalid", "ExpiresInDays must be 7 or 14"));
            }

            var now = _clock.UtcNow;
            var expiresAtUtc = now.AddDays(request.ExpiresInDays);
            var linkPayload = _invitationLinkService.CreateLink();

            var invitationResult = Invitation.Create(
                _user.UserId,
                linkPayload.TokenHash,
                now,
                expiresAtUtc);

            if (invitationResult.IsFailure)
            {
                return Result<CreateInvitationResult>.Failure(invitationResult.Error);
            }

            await _invitationRepository.AddAsync(invitationResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CreateInvitationResult>.Success(
                new CreateInvitationResult(
                    invitationResult.Value.Id,
                    linkPayload.InviteUrl,
                    expiresAtUtc));
        }
    }
}
