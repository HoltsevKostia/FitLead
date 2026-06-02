using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Time;
using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetTrainerClientOverviewSummaryHandler
        : IRequestHandler<GetTrainerClientOverviewSummaryQuery, Result<TrainerClientOverviewSummaryDto>>
    {
        private readonly ITrainerClientAccessLoader _accessLoader;
        private readonly ITrainerClientOverviewReadRepository _overviewReadRepository;
        private readonly IClock _clock;

        public GetTrainerClientOverviewSummaryHandler(
            ITrainerClientAccessLoader accessLoader,
            ITrainerClientOverviewReadRepository overviewReadRepository,
            IClock clock)
        {
            _accessLoader = accessLoader;
            _overviewReadRepository = overviewReadRepository;
            _clock = clock;
        }

        public async Task<Result<TrainerClientOverviewSummaryDto>> Handle(
            GetTrainerClientOverviewSummaryQuery request,
            CancellationToken cancellationToken)
        {
            var accessResult = await _accessLoader.GetAccessibleClientAsync(
                request.ClientId,
                cancellationToken);

            if (accessResult.IsFailure)
            {
                return Result<TrainerClientOverviewSummaryDto>.Failure(accessResult.Error);
            }

            var overview = await _overviewReadRepository.GetOverviewSummaryAsync(
                accessResult.Value.TrainerId,
                accessResult.Value.ClientId,
                _clock.UtcNow,
                cancellationToken);

            return Result<TrainerClientOverviewSummaryDto>.Success(overview);
        }
    }
}
