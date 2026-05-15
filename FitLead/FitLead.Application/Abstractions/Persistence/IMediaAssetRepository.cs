using FitLead.Domain.Media.MediaAssets;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IMediaAssetRepository
    {
        Task AddAsync(
            MediaAsset mediaAsset,
            CancellationToken cancellationToken);

        Task<MediaAsset?> GetByIdAsync(
            Guid mediaAssetId,
            CancellationToken cancellationToken);

        Task<MediaAsset?> GetOwnedByIdAsync(
            Guid ownerUserId,
            Guid mediaAssetId,
            CancellationToken cancellationToken);

        Task<MediaAsset?> GetByStorageObjectAsync(
            MediaStorageProvider storageProvider,
            string storageObjectId,
            CancellationToken cancellationToken);
    }
}
