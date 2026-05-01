using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Application.Common.Time;
using FitLead.Application.Modules.Users;
using FitLead.Domain.Invitations;
using FitLead.Domain.Users;
using MediatR;
using FitLead.Application.Identity;

namespace FitLead.Application.Invitations.Commands
{
    public sealed class CreateInvitationHandler 
        : IRequestHandler<CreateInvitationCommand, Result<Guid>>
    {
        private readonly IUserContext _user;
        private readonly IClock _clock;
        private readonly IUsersModule _usersModule;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInvitationRepository _invitationRepository;

        public CreateInvitationHandler(
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

        public async Task<Result<Guid>> Handle(
            CreateInvitationCommand request,
            CancellationToken cancellationToken)
        {
            var trainer = await _usersModule.GetByIdAsync(_user.UserId, cancellationToken);

            if (trainer is null)
                return Result<Guid>.Failure(Error.NotFound("trainer.not_found", "Trainer not found"));

            if (trainer.Role != UserRole.Trainer)
                return Result<Guid>.Failure(Error.Forbidden("trainer.required", "User is not a trainer"));

            var client = await _usersModule.GetByIdAsync(request.ClientId, cancellationToken);

            if (client is null)
                return Result<Guid>.Failure(Error.NotFound("client.not_found", "Client not found"));

            if (client.Role != UserRole.Client)
                return Result<Guid>.Failure(Error.Forbidden("client.required", "User is not a client"));

            var alreadyPending = await _invitationRepository
                .ExistsPendingAsync(
                    _user.UserId,
                    request.ClientId,
                    cancellationToken);

            if (alreadyPending)
                return Result<Guid>.Failure(Error.Conflict("invitation.pending", "Invitation already pending"));

            var sentToday = await _invitationRepository
                .CountSentByTrainerForDateAsync(
                    _user.UserId,
                    _clock.UtcNow,
                    cancellationToken);

            if (sentToday >= 2)
                return Result<Guid>.Failure(Error.Conflict("invitation.daily_limit", "Daily invitation limit reached"));

            var invitationResult = Invitation.Create(
                _user.UserId,
                request.ClientId,
                _clock.UtcNow);
            if (invitationResult.IsFailure)
                return Result<Guid>.Failure(invitationResult.Error);

            await _invitationRepository.AddAsync(invitationResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(invitationResult.Value.Id);
        }
    }
}
