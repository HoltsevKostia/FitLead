using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Time;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;
using MediatR;

namespace FitLead.Application.ClientDashboard.Queries
{
    public sealed class GetClientDashboardHandler
        : IRequestHandler<GetClientDashboardQuery, Result<ClientDashboardDto>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IClientDashboardReadRepository _dashboardReadRepository;
        private readonly IClock _clock;

        public GetClientDashboardHandler(
            ICurrentUserLoader currentUserLoader,
            IClientDashboardReadRepository dashboardReadRepository,
            IClock clock)
        {
            _currentUserLoader = currentUserLoader;
            _dashboardReadRepository = dashboardReadRepository;
            _clock = clock;
        }

        public async Task<Result<ClientDashboardDto>> Handle(
            GetClientDashboardQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserResult =
                await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<ClientDashboardDto>.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Client)
            {
                return Result<ClientDashboardDto>.Failure(
                    Error.Forbidden("client.required", "User is not a client"));
            }

            var dashboard = await _dashboardReadRepository.GetAsync(
                currentUserResult.Value.Id,
                _clock.UtcNow,
                cancellationToken);

            return Result<ClientDashboardDto>.Success(dashboard);
        }
    }
}
