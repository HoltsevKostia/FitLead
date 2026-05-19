using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Media.MediaAssets.Queries
{
    public sealed class GetMyMediaAssetsHandler
        : IRequestHandler<GetMyMediaAssetsQuery, Result<IReadOnlyList<MediaAssetDto>>>
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IMediaAssetReadRepository _mediaAssetReadRepository;

        public GetMyMediaAssetsHandler(
            ICurrentUserLoader currentUserLoader,
            IMediaAssetReadRepository mediaAssetReadRepository)
        {
            _currentUserLoader = currentUserLoader;
            _mediaAssetReadRepository = mediaAssetReadRepository;
        }

        public async Task<Result<IReadOnlyList<MediaAssetDto>>> Handle(
            GetMyMediaAssetsQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<IReadOnlyList<MediaAssetDto>>.Failure(currentUserResult.Error);
            }

            var mediaAssets = await _mediaAssetReadRepository.GetActiveOwnedByUserAsync(
                currentUserResult.Value.Id,
                cancellationToken);

            return Result<IReadOnlyList<MediaAssetDto>>.Success(mediaAssets);
        }
    }
}
