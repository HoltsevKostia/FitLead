using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Users.Queries
{
    public sealed class GetTrainerClientProgressHandler
        : IRequestHandler<GetTrainerClientProgressQuery, Result<TrainerClientProgressDto>>
    {
        private readonly ITrainerClientAccessLoader _accessLoader;
        private readonly ITrainerClientProgressReadRepository _progressReadRepository;

        public GetTrainerClientProgressHandler(
            ITrainerClientAccessLoader accessLoader,
            ITrainerClientProgressReadRepository progressReadRepository)
        {
            _accessLoader = accessLoader;
            _progressReadRepository = progressReadRepository;
        }

        public async Task<Result<TrainerClientProgressDto>> Handle(
            GetTrainerClientProgressQuery request,
            CancellationToken cancellationToken)
        {
            var accessResult = await _accessLoader.GetAccessibleClientAsync(
                request.ClientId,
                cancellationToken);

            if (accessResult.IsFailure)
            {
                return Result<TrainerClientProgressDto>.Failure(accessResult.Error);
            }

            var progress = await _progressReadRepository.GetProgressAsync(
                accessResult.Value.TrainerId,
                accessResult.Value.ClientId,
                cancellationToken);

            return Result<TrainerClientProgressDto>.Success(progress);
        }
    }
}
