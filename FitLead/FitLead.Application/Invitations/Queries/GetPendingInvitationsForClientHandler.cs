using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Identity;
using FitLead.Common.Results;
using FitLead.Application.Common.Time;
using MediatR;

namespace FitLead.Application.Invitations.Queries
{
    public sealed class GetPendingInvitationsForClientHandler
    : IRequestHandler<GetPendingInvitationsForClientQuery, Result<IReadOnlyList<InvitationDto>>>
    {
        private readonly IUserContext _user;
        private readonly IClock _clock;
        private readonly IInvitationReadRepository _repository;

        public GetPendingInvitationsForClientHandler(
            IUserContext user,
            IClock clock,
            IInvitationReadRepository repository)
        {
            _user = user;
            _clock = clock;
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<InvitationDto>>> Handle(
            GetPendingInvitationsForClientQuery request,
            CancellationToken cancellationToken)
        {
            var invitations = await _repository.GetPendingForClientAsync(
                _user.UserId,
                _clock.UtcNow,
                cancellationToken);
            return Result<IReadOnlyList<InvitationDto>>.Success(invitations);
        }
    }
}
