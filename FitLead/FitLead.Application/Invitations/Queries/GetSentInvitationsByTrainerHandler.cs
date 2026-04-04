using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Identity;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Invitations.Queries
{
    public sealed class GetSentInvitationsByTrainerHandler
    : IRequestHandler<GetSentInvitationsByTrainerQuery, Result<IReadOnlyList<InvitationDto>>>
    {
        private readonly IUserContext _user;
        private readonly IInvitationReadRepository _repository;

        public GetSentInvitationsByTrainerHandler(
            IUserContext user,
            IInvitationReadRepository repository)
        {
            _user = user;
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<InvitationDto>>> Handle(
            GetSentInvitationsByTrainerQuery request,
            CancellationToken cancellationToken)
        {
            var invitations = await _repository.GetSentByTrainerAsync(
                _user.UserId,
                cancellationToken);
            return Result<IReadOnlyList<InvitationDto>>.Success(invitations);
        }
    }
}
