using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Clients.ProgressPhotos;
using FitLead.Application.Media.MediaAssets.Queries;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class ClientProgressPhotoReadRepository : IClientProgressPhotoReadRepository
    {
        private readonly FitLeadDbContext _context;

        public ClientProgressPhotoReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<ClientProgressPhotoDto>> GetByClientAsync(
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var photos = await _context.ClientProgressPhotos
                .AsNoTracking()
                .Where(photo => photo.ClientId == clientId)
                .Join(
                    _context.MediaAssets.AsNoTracking(),
                    photo => photo.MediaAssetId,
                    mediaAsset => mediaAsset.Id,
                    (photo, mediaAsset) => new
                    {
                        Photo = photo,
                        MediaAsset = mediaAsset
                    })
                .OrderByDescending(item => item.Photo.TakenAt)
                .ThenByDescending(item => item.Photo.Id)
                .ToListAsync(cancellationToken);

            return photos
                .Select(item => new ClientProgressPhotoDto(
                    item.Photo.Id,
                    item.Photo.ClientId,
                    item.Photo.MediaAssetId,
                    new MediaAssetPreviewDto(
                        item.MediaAsset.Id,
                        item.MediaAsset.DeliveryUrl,
                        item.MediaAsset.FileName,
                        item.MediaAsset.ContentType,
                        item.MediaAsset.SizeBytes,
                        item.MediaAsset.Kind.ToString(),
                        item.MediaAsset.DurationSeconds),
                    item.Photo.TakenAt,
                    item.Photo.Label.ToString(),
                    item.Photo.Note,
                    item.Photo.CreatedAtUtc))
                .ToList();
        }
    }
}
