using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Time;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.TrainerDashboard.Queries
{
    public sealed class GetTrainerDashboardSummaryHandler
        : IRequestHandler<GetTrainerDashboardSummaryQuery, Result<TrainerDashboardSummaryDto>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly ITrainerDashboardReadRepository _dashboardReadRepository;
        private readonly IClock _clock;

        public GetTrainerDashboardSummaryHandler(
            ICurrentUserLoader currentUserLoader,
            ITrainerDashboardReadRepository dashboardReadRepository,
            IClock clock)
        {
            _currentUserLoader = currentUserLoader;
            _dashboardReadRepository = dashboardReadRepository;
            _clock = clock;
        }

        public async Task<Result<TrainerDashboardSummaryDto>> Handle(
            GetTrainerDashboardSummaryQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserResult =
                await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<TrainerDashboardSummaryDto>.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Trainer)
            {
                return Result<TrainerDashboardSummaryDto>.Failure(
                    Error.Forbidden("trainer.required", "User is not a trainer"));
            }

            var summary = await _dashboardReadRepository.GetSummaryAsync(
                currentUserResult.Value.Id,
                _clock.UtcNow,
                cancellationToken);

            return Result<TrainerDashboardSummaryDto>.Success(summary);
        }
    }
}
