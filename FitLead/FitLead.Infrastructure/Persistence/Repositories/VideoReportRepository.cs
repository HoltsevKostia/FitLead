using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Messenger.VideoReports;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class VideoReportRepository : IVideoReportRepository
    {
        private readonly FitLeadDbContext _context;

        public VideoReportRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            VideoReport videoReport,
            CancellationToken cancellationToken)
        {
            await _context.VideoReports.AddAsync(videoReport, cancellationToken);
        }
    }
}
