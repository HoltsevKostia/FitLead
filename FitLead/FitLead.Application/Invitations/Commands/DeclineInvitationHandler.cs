using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Invitations.Commands
{
    public sealed class DeclineInvitationHandler
    : IRequestHandler<DeclineInvitationCommand, Result>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeclineInvitationHandler(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork)
        {
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
                return Result.Failure("Invitation not found");

            if (invitation.ClientId != request.ClientId)
                return Result.Failure("Forbidden");

            invitation.Decline(request.Now);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
