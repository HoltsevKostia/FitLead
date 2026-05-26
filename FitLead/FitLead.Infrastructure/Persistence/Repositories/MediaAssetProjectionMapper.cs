using FitLead.Application.Media.MediaAssets.Queries;
using FitLead.Domain.Media.MediaAssets;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    internal static class MediaAssetProjectionMapper
    {
        public static MediaAssetPreviewDto? ToPreviewDto(MediaAsset? mediaAsset)
        {
            return mediaAsset is null
                ? null
                : new MediaAssetPreviewDto(
                    mediaAsset.Id,
                    mediaAsset.DeliveryUrl,
                    mediaAsset.FileName,
                    mediaAsset.ContentType,
                    mediaAsset.SizeBytes,
                    mediaAsset.Kind.ToString(),
                    mediaAsset.DurationSeconds);
        }

        public static MediaAssetDto ToDto(MediaAsset mediaAsset)
        {
            return new MediaAssetDto(
                mediaAsset.Id,
                mediaAsset.StorageProvider.ToString(),
                mediaAsset.StorageObjectId,
                mediaAsset.DeliveryUrl,
                mediaAsset.FileName,
                mediaAsset.ContentType,
                mediaAsset.SizeBytes,
                mediaAsset.Kind.ToString(),
                mediaAsset.DurationSeconds,
                mediaAsset.Status.ToString(),
                mediaAsset.CreatedAtUtc);
        }
    }
}
