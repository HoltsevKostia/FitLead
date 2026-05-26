using FitLead.Application.Media.MediaAssets.Queries;
using FitLead.Domain.Clients.ProgressPhotos;
using FitLead.Domain.Media.MediaAssets;

namespace FitLead.Application.Clients.ProgressPhotos
{
    public static class ClientProgressPhotoMapping
    {
        public static ClientProgressPhotoDto ToDto(
            ClientProgressPhoto photo,
            MediaAsset mediaAsset)
        {
            return new ClientProgressPhotoDto(
                photo.Id,
                photo.ClientId,
                photo.MediaAssetId,
                new MediaAssetPreviewDto(
                    mediaAsset.Id,
                    mediaAsset.DeliveryUrl,
                    mediaAsset.FileName,
                    mediaAsset.ContentType,
                    mediaAsset.SizeBytes,
                    mediaAsset.Kind.ToString(),
                    mediaAsset.DurationSeconds),
                photo.TakenAt,
                photo.Label.ToString(),
                photo.Note,
                photo.CreatedAtUtc);
        }
    }
}
