using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Domain.Invitations;
using FitLead.Domain.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Commands.AcceptInvitation
{
    public sealed class AcceptInvitationHandler
        : IRequestHandler<AcceptInvitationCommand, Result>
    {
        private readonly IUserRepository _userRepository;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AcceptInvitationHandler(IUserRepository userRepository, IInvitationRepository invitationRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _invitationRepository = invitationRepository;
        }

        public async Task<Result> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
        {
            var client = await _userRepository.GetByIdAsync(request.ClientId, cancellationToken);

            if (client is null)
                return Result.Failure("Client not found");

            if (client.Role != UserRole.Client)
                return Result.Failure("User is not a Client");

            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId, cancellationToken);
            
            if (invitation is null)
                return Result.Failure("Invitation not found");
            
            if (invitation.ClientId != request.ClientId)
                return Result.Failure("Invitation does not belong to this client");

            invitation.Accept(request.Now);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
