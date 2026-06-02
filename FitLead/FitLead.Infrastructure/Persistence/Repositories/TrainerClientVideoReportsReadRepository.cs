using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Users.Queries;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class TrainerClientVideoReportsReadRepository
        : ITrainerClientVideoReportsReadRepository
    {
        private readonly FitLeadDbContext _context;

        public TrainerClientVideoReportsReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TrainerClientVideoReportDto>> GetRecentVideoReportsAsync(
            Guid trainerId,
            Guid clientId,
            int limit,
            CancellationToken cancellationToken)
        {
            if (limit <= 0)
            {
                return Array.Empty<TrainerClientVideoReportDto>();
            }

            var reports = await _context.VideoReports
                .AsNoTracking()
                .Where(videoReport =>
                    videoReport.TrainerId == trainerId &&
                    videoReport.ClientId == clientId)
                .OrderByDescending(videoReport => videoReport.CreatedAtUtc)
                .ThenByDescending(videoReport => videoReport.Id)
                .Select(videoReport => new
                {
                    ReportId = videoReport.Id,
                    videoReport.ChatId,
                    videoReport.Title,
                    videoReport.Description,
                    videoReport.Status,
                    MediaCount = _context.VideoReportMedia.Count(media =>
                        media.VideoReportId == videoReport.Id),
                    videoReport.CreatedAtUtc,
                    videoReport.ReviewedAtUtc
                })
                .Take(limit)
                .ToListAsync(cancellationToken);

            return reports
                .Select(report => new TrainerClientVideoReportDto(
                    report.ReportId,
                    report.ChatId,
                    report.Title,
                    report.Description,
                    report.Status.ToString(),
                    report.MediaCount,
                    report.CreatedAtUtc,
                    report.ReviewedAtUtc))
                .ToList();
        }
    }
}
