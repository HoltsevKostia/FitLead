using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Messenger.VideoReports.Queries;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class VideoReportReadRepository : IVideoReportReadRepository
    {
        private readonly FitLeadDbContext _context;

        public VideoReportReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<VideoReportDetailsDto?> GetDetailsAsync(
            Guid chatId,
            Guid reportId,
            CancellationToken cancellationToken)
        {
            var report = await _context.VideoReports
                .AsNoTracking()
                .Where(videoReport =>
                    videoReport.Id == reportId &&
                    videoReport.ChatId == chatId)
                .Select(videoReport => new
                {
                    videoReport.Id,
                    videoReport.ChatId,
                    videoReport.ClientId,
                    videoReport.TrainerId,
                    videoReport.Title,
                    videoReport.Description,
                    videoReport.Status,
                    videoReport.CreatedAtUtc,
                    videoReport.ReviewedAtUtc,
                    videoReport.TrainerFeedbackText
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (report is null)
            {
                return null;
            }

            var media = await (
                from videoReportMedia in _context.VideoReportMedia.AsNoTracking()
                join mediaAsset in _context.MediaAssets.AsNoTracking()
                    on videoReportMedia.MediaAssetId equals mediaAsset.Id
                where videoReportMedia.VideoReportId == report.Id
                orderby videoReportMedia.OrderInReport
                select new VideoReportMediaDto(
                    mediaAsset.Id,
                    mediaAsset.DeliveryUrl,
                    mediaAsset.FileName,
                    mediaAsset.ContentType,
                    mediaAsset.SizeBytes,
                    mediaAsset.Kind.ToString(),
                    mediaAsset.DurationSeconds,
                    videoReportMedia.OrderInReport))
                .ToListAsync(cancellationToken);

            return new VideoReportDetailsDto(
                report.Id,
                report.ChatId,
                report.ClientId,
                report.TrainerId,
                report.Title,
                report.Description,
                report.Status.ToString(),
                report.CreatedAtUtc,
                report.ReviewedAtUtc,
                report.TrainerFeedbackText,
                media);
        }
    }
}
