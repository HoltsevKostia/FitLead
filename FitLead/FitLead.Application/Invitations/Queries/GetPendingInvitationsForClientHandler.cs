using FitLead.Application.Abstractions.Persistence;
using MediatR;


namespace FitLead.Application.Invitations.Queries
{
    public sealed class GetPendingInvitationsForClientHandler
    : IRequestHandler<GetPendingInvitationsForClientQuery, IReadOnlyList<InvitationDto>>
    {
        private readonly IInvitationReadRepository _repository;

        public GetPendingInvitationsForClientHandler(
            IInvitationReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<InvitationDto>> Handle(
            GetPendingInvitationsForClientQuery request,
            CancellationToken cancellationToken)
        {
            return await _repository.GetPendingForClientAsync(
                request.ClientId,
                request.Now,
                cancellationToken);
        }
    }
}
