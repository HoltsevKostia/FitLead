using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Time;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetTrainerClientsOverviewHandler
        : IRequestHandler<GetTrainerClientsOverviewQuery, Result<IReadOnlyList<TrainerClientOverviewDto>>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly ITrainerClientReadRepository _repository;
        private readonly IClock _clock;

        public GetTrainerClientsOverviewHandler(
            ICurrentUserLoader currentUserLoader,
            ITrainerClientReadRepository repository,
            IClock clock)
        {
            _currentUserLoader = currentUserLoader;
            _repository = repository;
            _clock = clock;
        }

        public async Task<Result<IReadOnlyList<TrainerClientOverviewDto>>> Handle(
            GetTrainerClientsOverviewQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<IReadOnlyList<TrainerClientOverviewDto>>.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Trainer)
            {
                return Result<IReadOnlyList<TrainerClientOverviewDto>>.Failure(
                    Error.Forbidden("trainer.required", "User is not a trainer"));
            }

            var clients = await _repository.GetClientsOverviewByTrainerIdAsync(
                currentUserResult.Value.Id,
                _clock.UtcNow,
                cancellationToken);

            return Result<IReadOnlyList<TrainerClientOverviewDto>>.Success(clients);
        }
    }
}
