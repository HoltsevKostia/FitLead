using FitLead.Application.Abstractions.Persistence;
using MediatR;


namespace FitLead.Application.Invitations.Queries
{
    public sealed class GetSentInvitationsByTrainerHandler
    : IRequestHandler<GetSentInvitationsByTrainerQuery, IReadOnlyList<InvitationDto>>
    {
        private readonly IInvitationReadRepository _repository;

        public GetSentInvitationsByTrainerHandler(
            IInvitationReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<InvitationDto>> Handle(
            GetSentInvitationsByTrainerQuery request,
            CancellationToken cancellationToken)
        {
            return await _repository.GetSentByTrainerAsync(
                request.TrainerId,
                cancellationToken);
        }
    }
}
