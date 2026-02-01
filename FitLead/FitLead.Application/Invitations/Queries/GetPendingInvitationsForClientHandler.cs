using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Identity;
using FitLead.Application.Common.Time;
using MediatR;


namespace FitLead.Application.Invitations.Queries
{
    public sealed class GetPendingInvitationsForClientHandler
    : IRequestHandler<GetPendingInvitationsForClientQuery, IReadOnlyList<InvitationDto>>
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

        public async Task<IReadOnlyList<InvitationDto>> Handle(
            GetPendingInvitationsForClientQuery request,
            CancellationToken cancellationToken)
        {
            return await _repository.GetPendingForClientAsync(
                _user.UserId,
                _clock.UtcNow,
                cancellationToken);
        }
    }
}
