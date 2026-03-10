using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetClientsByTrainerIdHandler
    : IRequestHandler<GetClientsByTrainerIdQuery, Result<IReadOnlyList<TrainerClientDto>>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly ITrainerClientReadRepository _repository;

        public GetClientsByTrainerIdHandler(
            ICurrentUserLoader currentUserLoader,
            ITrainerClientReadRepository repository)
        {
            _currentUserLoader = currentUserLoader;
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<TrainerClientDto>>> Handle(
            GetClientsByTrainerIdQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
                return Result<IReadOnlyList<TrainerClientDto>>.Failure(currentUserResult.Error);

            if (currentUserResult.Value.Role != UserRole.Trainer)
            {
                return Result<IReadOnlyList<TrainerClientDto>>.Failure(
                    Error.Forbidden("trainer.required", "User is not a trainer"));
            }

            var clients = await _repository.GetClientsByTrainerIdAsync(
                currentUserResult.Value.Id,
                cancellationToken);

            return Result<IReadOnlyList<TrainerClientDto>>.Success(clients);
        }
    }
}
