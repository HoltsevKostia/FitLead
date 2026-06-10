using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.TrainerVideoReports.Queries;
using FitLead.Domain.Messenger.VideoReports;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class TrainerPendingVideoReportsReadRepository
        : ITrainerPendingVideoReportsReadRepository
    {
        private readonly FitLeadDbContext _context;

        public TrainerPendingVideoReportsReadRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TrainerPendingVideoReportDto>> GetPendingAsync(
            Guid trainerId,
            CancellationToken cancellationToken)
        {
            return await (
                from report in _context.VideoReports.AsNoTracking()
                join client in _context.DomainUsers.AsNoTracking()
                    on report.ClientId equals client.Id
                where report.TrainerId == trainerId &&
                      report.Status == VideoReportStatus.Submitted
                orderby report.CreatedAtUtc, report.Id
                select new TrainerPendingVideoReportDto(
                    report.Id,
                    report.ChatId,
                    report.ClientId,
                    client.FullName,
                    report.Title,
                    report.Description,
                    _context.VideoReportMedia.Count(media =>
                        media.VideoReportId == report.Id),
                    report.CreatedAtUtc))
                .ToListAsync(cancellationToken);
        }
    }
}
