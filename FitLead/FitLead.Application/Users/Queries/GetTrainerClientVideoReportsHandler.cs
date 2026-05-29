using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetTrainerClientVideoReportsHandler
        : IRequestHandler<GetTrainerClientVideoReportsQuery, Result<IReadOnlyList<TrainerClientVideoReportDto>>>
    {
        private const int RecentVideoReportsLimit = 5;

        private readonly ITrainerClientAccessLoader _accessLoader;
        private readonly ITrainerClientVideoReportsReadRepository _videoReportsReadRepository;

        public GetTrainerClientVideoReportsHandler(
            ITrainerClientAccessLoader accessLoader,
            ITrainerClientVideoReportsReadRepository videoReportsReadRepository)
        {
            _accessLoader = accessLoader;
            _videoReportsReadRepository = videoReportsReadRepository;
        }

        public async Task<Result<IReadOnlyList<TrainerClientVideoReportDto>>> Handle(
            GetTrainerClientVideoReportsQuery request,
            CancellationToken cancellationToken)
        {
            var accessResult = await _accessLoader.GetAccessibleClientAsync(
                request.ClientId,
                cancellationToken);

            if (accessResult.IsFailure)
            {
                return Result<IReadOnlyList<TrainerClientVideoReportDto>>.Failure(accessResult.Error);
            }

            var reports = await _videoReportsReadRepository.GetRecentVideoReportsAsync(
                accessResult.Value.TrainerId,
                accessResult.Value.ClientId,
                RecentVideoReportsLimit,
                cancellationToken);

            return Result<IReadOnlyList<TrainerClientVideoReportDto>>.Success(reports);
        }
    }
}
