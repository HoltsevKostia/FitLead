using FitLead.Domain.Messenger.VideoReports;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IVideoReportRepository
    {
        Task AddAsync(
            VideoReport videoReport,
            CancellationToken cancellationToken);
    }
}
