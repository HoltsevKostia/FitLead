using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Identity;
using MediatR;


namespace FitLead.Application.Invitations.Queries
{
    public sealed class GetSentInvitationsByTrainerHandler
    : IRequestHandler<GetSentInvitationsByTrainerQuery, IReadOnlyList<InvitationDto>>
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

        public async Task<IReadOnlyList<InvitationDto>> Handle(
            GetSentInvitationsByTrainerQuery request,
            CancellationToken cancellationToken)
        {
            return await _repository.GetSentByTrainerAsync(
                _user.UserId,
                cancellationToken);
        }
    }
}
