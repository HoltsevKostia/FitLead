using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetTrainerClientWorkoutLogsHandler
        : IRequestHandler<GetTrainerClientWorkoutLogsQuery, Result<IReadOnlyList<TrainerClientWorkoutLogDto>>>
    {
        private const int RecentWorkoutLogsLimit = 50;

        private readonly ITrainerClientAccessLoader _accessLoader;
        private readonly ITrainerClientWorkoutLogsReadRepository _workoutLogsReadRepository;

        public GetTrainerClientWorkoutLogsHandler(
            ITrainerClientAccessLoader accessLoader,
            ITrainerClientWorkoutLogsReadRepository workoutLogsReadRepository)
        {
            _accessLoader = accessLoader;
            _workoutLogsReadRepository = workoutLogsReadRepository;
        }

        public async Task<Result<IReadOnlyList<TrainerClientWorkoutLogDto>>> Handle(
            GetTrainerClientWorkoutLogsQuery request,
            CancellationToken cancellationToken)
        {
            var accessResult = await _accessLoader.GetAccessibleClientAsync(
                request.ClientId,
                cancellationToken);

            if (accessResult.IsFailure)
            {
                return Result<IReadOnlyList<TrainerClientWorkoutLogDto>>.Failure(accessResult.Error);
            }

            var logs = await _workoutLogsReadRepository.GetRecentWorkoutLogsAsync(
                accessResult.Value.TrainerId,
                accessResult.Value.ClientId,
                RecentWorkoutLogsLimit,
                cancellationToken);

            return Result<IReadOnlyList<TrainerClientWorkoutLogDto>>.Success(logs);
        }
    }
}
