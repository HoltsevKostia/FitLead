using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Application.Common.Time;
using FitLead.Application.Invitations.Access;
using FitLead.Application.Modules.Users;
using FitLead.Domain.Users;
using MediatR;
using FitLead.Application.Identity;

namespace FitLead.Application.Invitations.Commands
{
    public sealed class AcceptInvitationHandler
        : IRequestHandler<AcceptInvitationCommand, Result>
    {
        private readonly IUserContext _user;
        private readonly IClock _clock;
        private readonly IUsersModule _usersModule;
        private readonly IInvitationLoader _invitationLoader;
        private readonly IUnitOfWork _unitOfWork;

        public AcceptInvitationHandler(
            IUserContext user,
            IClock clock,
            IUsersModule usersModule,
            IInvitationLoader invitationLoader,
            IUnitOfWork unitOfWork)
        {
            _user = user;
            _clock = clock;
            _usersModule = usersModule;
            _unitOfWork = unitOfWork;
            _invitationLoader = invitationLoader;
        }

        public async Task<Result> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
        {
            var client = await _usersModule.GetByIdAsync(_user.UserId, cancellationToken);

            if (client is null)
                return Result.Failure(Error.NotFound("client.not_found", "Client not found"));

            if (client.Role != UserRole.Client)
                return Result.Failure(Error.Forbidden("client.required", "User is not a client"));

            var invitationResult = await _invitationLoader.GetClientOwnedOrNotFoundAsync(
                request.InvitationId,
                cancellationToken);
            if (invitationResult.IsFailure)
                return Result.Failure(invitationResult.Error);

            var invitation = invitationResult.Value;
            var acceptResult = invitation.Accept(_clock.UtcNow);
            if (acceptResult.IsFailure)
                return acceptResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
