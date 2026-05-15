using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Media.MediaAssets;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class MediaAssetRepository : IMediaAssetRepository
    {
        private readonly FitLeadDbContext _context;

        public MediaAssetRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            MediaAsset mediaAsset,
            CancellationToken cancellationToken)
        {
            await _context.MediaAssets.AddAsync(mediaAsset, cancellationToken);
        }

        public async Task<MediaAsset?> GetByIdAsync(
            Guid mediaAssetId,
            CancellationToken cancellationToken)
        {
            return await _context.MediaAssets
                .FirstOrDefaultAsync(mediaAsset => mediaAsset.Id == mediaAssetId, cancellationToken);
        }

        public async Task<MediaAsset?> GetOwnedByIdAsync(
            Guid ownerUserId,
            Guid mediaAssetId,
            CancellationToken cancellationToken)
        {
            return await _context.MediaAssets
                .FirstOrDefaultAsync(
                    mediaAsset => mediaAsset.OwnerUserId == ownerUserId &&
                                  mediaAsset.Id == mediaAssetId,
                    cancellationToken);
        }

        public async Task<MediaAsset?> GetByStorageObjectAsync(
            MediaStorageProvider storageProvider,
            string storageObjectId,
            CancellationToken cancellationToken)
        {
            return await _context.MediaAssets
                .FirstOrDefaultAsync(
                    mediaAsset => mediaAsset.StorageProvider == storageProvider &&
                                  mediaAsset.StorageObjectId == storageObjectId,
                    cancellationToken);
        }
    }
}
