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
    public sealed class ExpireInvitationsHandler
    : IRequestHandler<ExpireInvitationsCommand, Result>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ExpireInvitationsHandler(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork)
        {
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            ExpireInvitationsCommand request,
            CancellationToken cancellationToken)
        {
            var invitations = await _invitationRepository
                .GetExpiredPendingAsync(request.Now, cancellationToken);

            foreach (var invitation in invitations)
            {
                invitation.Expire(request.Now);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

}
