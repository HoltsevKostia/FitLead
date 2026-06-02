using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Clients.BodyMetrics;
using FitLead.Application.Clients.ProgressPhotos;
using FitLead.Application.Users.Queries;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class TrainerClientProgressReadRepository
        : ITrainerClientProgressReadRepository
    {
        private readonly FitLeadDbContext _context;

        public TrainerClientProgressReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<TrainerClientProgressDto> GetProgressAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var metrics = await _context.ClientBodyMetricEntries
                .AsNoTracking()
                .Where(entry => entry.ClientId == clientId)
                .OrderByDescending(entry => entry.RecordedAt)
                .ThenByDescending(entry => entry.CreatedAtUtc)
                .ThenByDescending(entry => entry.Id)
                .Select(entry => new ClientBodyMetricEntryDto(
                    entry.Id,
                    entry.ClientId,
                    entry.RecordedAt,
                    entry.WeightKg,
                    entry.BodyFatPercent,
                    entry.ChestCm,
                    entry.WaistCm,
                    entry.HipsCm,
                    entry.ArmCm,
                    entry.ThighCm,
                    entry.Note,
                    entry.CreatedAtUtc,
                    entry.UpdatedAtUtc))
                .ToListAsync(cancellationToken);

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
                .ThenByDescending(item => item.Photo.CreatedAtUtc)
                .ThenByDescending(item => item.Photo.Id)
                .Select(item => new ClientProgressPhotoDto(
                    item.Photo.Id,
                    item.Photo.ClientId,
                    item.Photo.MediaAssetId,
                    MediaAssetProjectionMapper.ToPreviewDto(item.MediaAsset)!,
                    item.Photo.TakenAt,
                    item.Photo.Label.ToString(),
                    item.Photo.Note,
                    item.Photo.CreatedAtUtc))
                .ToListAsync(cancellationToken);

            return new TrainerClientProgressDto(metrics, photos);
        }
    }
}
