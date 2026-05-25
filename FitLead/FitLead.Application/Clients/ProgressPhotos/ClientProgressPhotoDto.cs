using FitLead.Application.Media.MediaAssets.Queries;

namespace FitLead.Application.Clients.ProgressPhotos
{
    public sealed record ClientProgressPhotoDto(
        Guid Id,
        Guid ClientId,
        Guid MediaAssetId,
        MediaAssetPreviewDto MediaAsset,
        DateOnly TakenAt,
        string Label,
        string? Note,
        DateTime CreatedAtUtc);
}
