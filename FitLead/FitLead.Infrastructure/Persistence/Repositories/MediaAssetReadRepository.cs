using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Media.MediaAssets.Queries;
using FitLead.Domain.Media.MediaAssets;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class MediaAssetReadRepository : IMediaAssetReadRepository
    {
        private readonly FitLeadDbContext _context;

        public MediaAssetReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<MediaAssetDto>> GetActiveOwnedByUserAsync(
            Guid ownerUserId,
            CancellationToken cancellationToken)
        {
            return await _context.MediaAssets
                .AsNoTracking()
                .Where(mediaAsset =>
                    mediaAsset.OwnerUserId == ownerUserId &&
                    mediaAsset.Status == MediaAssetStatus.Active)
                .OrderByDescending(mediaAsset => mediaAsset.CreatedAtUtc)
                .Select(mediaAsset => new MediaAssetDto(
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
                    mediaAsset.CreatedAtUtc))
                .ToListAsync(cancellationToken);
        }
    }
}
