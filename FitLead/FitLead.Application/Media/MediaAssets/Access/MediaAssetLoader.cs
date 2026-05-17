using FitLead.Application.Abstractions.Persistence;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Media.MediaAssets;

namespace FitLead.Application.Media.MediaAssets.Access
{
    public sealed class MediaAssetLoader : IMediaAssetLoader
    {
        private static readonly Error MediaAssetNotFound =
            Error.NotFound("media_asset.not_found", "Media asset not found");

        private static readonly Error MediaAssetKindNotAllowed =
            Error.Validation("media_asset.kind_not_allowed_for_video_report", "Media asset kind is not allowed for video report");

        private readonly IMediaAssetRepository _mediaAssetRepository;

        public MediaAssetLoader(IMediaAssetRepository mediaAssetRepository)
        {
            _mediaAssetRepository = mediaAssetRepository;
        }

        public async Task<Result<MediaAsset>> GetOwnedOrNotFoundAsync(
            Guid ownerUserId,
            Guid mediaAssetId,
            CancellationToken cancellationToken)
        {
            var mediaAsset = await _mediaAssetRepository.GetOwnedByIdAsync(
                ownerUserId,
                mediaAssetId,
                cancellationToken);

            return mediaAsset is null
                ? Result<MediaAsset>.Failure(MediaAssetNotFound)
                : Result<MediaAsset>.Success(mediaAsset);
        }

        public async Task<Result<IReadOnlyList<MediaAsset>>> GetOwnedAllowedForVideoReportOrNotFoundAsync(
            Guid ownerUserId,
            IReadOnlyCollection<Guid> mediaAssetIds,
            CancellationToken cancellationToken)
        {
            var distinctMediaAssetIds = mediaAssetIds.Distinct().ToArray();
            var mediaAssets = await _mediaAssetRepository.GetOwnedByIdsAsync(
                ownerUserId,
                distinctMediaAssetIds,
                cancellationToken);

            if (mediaAssets.Count != distinctMediaAssetIds.Length)
            {
                return Result<IReadOnlyList<MediaAsset>>.Failure(MediaAssetNotFound);
            }

            if (mediaAssets.Any(mediaAsset => !IsAllowedForVideoReport(mediaAsset.Kind)))
            {
                return Result<IReadOnlyList<MediaAsset>>.Failure(MediaAssetKindNotAllowed);
            }

            return Result<IReadOnlyList<MediaAsset>>.Success(mediaAssets);
        }

        private static bool IsAllowedForVideoReport(MediaAssetKind kind)
        {
            return kind is MediaAssetKind.Image or MediaAssetKind.Video;
        }
    }
}
