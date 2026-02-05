using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Errors;
using FitLead.Application.Common.Identity;
using FitLead.Application.Common.Results;
using FitLead.Application.Common.Time;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Invitations.Commands
{
    public sealed class AcceptInvitationHandler
        : IRequestHandler<AcceptInvitationCommand, Result>
    {
        private readonly IUserContext _user;
        private readonly IClock _clock;
        private readonly IUserRepository _userRepository;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AcceptInvitationHandler(
            IUserContext user,
            IClock clock,
            IUserRepository userRepository,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork)
        {
            _user = user;
            _clock = clock;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _invitationRepository = invitationRepository;
        }

        public async Task<Result> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
        {
            var client = await _userRepository.GetByIdAsync(_user.UserId, cancellationToken);

            if (client is null)
                return Result.Failure(Error.NotFound("client.not_found", "Client not found"));

            if (client.Role != UserRole.Client)
                return Result.Failure(Error.Forbidden("client.required", "User is not a client"));

            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId, cancellationToken);
            
            if (invitation is null)
                return Result.Failure(Error.NotFound("invitation.not_found", "Invitation not found"));
            
            if (invitation.ClientId != _user.UserId)
                return Result.Failure(Error.Forbidden("invitation.forbidden", "Invitation does not belong to this client"));

            invitation.Accept(_clock.UtcNow);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
