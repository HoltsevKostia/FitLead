using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Common.Results;
using FitLead.Application.Common.Time;
using MediatR;

namespace FitLead.Application.Invitations.Commands
{
    public sealed class ExpireInvitationsHandler
    : IRequestHandler<ExpireInvitationsCommand, Result>
    {
        private readonly IClock _clock;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ExpireInvitationsHandler(
            IClock clock,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork)
        {
            _clock = clock;
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            ExpireInvitationsCommand request,
            CancellationToken cancellationToken)
        {
            var invitations = await _invitationRepository
                .GetExpiredPendingAsync(_clock.UtcNow, cancellationToken);

            foreach (var invitation in invitations)
            {
                invitation.Expire(_clock.UtcNow);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

}
