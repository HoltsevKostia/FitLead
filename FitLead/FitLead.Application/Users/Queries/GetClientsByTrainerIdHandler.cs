using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Identity;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetClientsByTrainerIdHandler
    : IRequestHandler<GetClientsByTrainerIdQuery, Result<IReadOnlyList<TrainerClientDto>>>
    {
        private readonly IUserContext _user;
        private readonly ITrainerClientReadRepository _repository;

        public GetClientsByTrainerIdHandler(
            IUserContext user,
            ITrainerClientReadRepository repository)
        {
            _user = user;
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<TrainerClientDto>>> Handle(
            GetClientsByTrainerIdQuery request,
            CancellationToken cancellationToken)
        {
            var clients = await _repository.GetClientsByTrainerIdAsync(
                _user.UserId,
                cancellationToken);
            return Result<IReadOnlyList<TrainerClientDto>>.Success(clients);
        }
    }
}
