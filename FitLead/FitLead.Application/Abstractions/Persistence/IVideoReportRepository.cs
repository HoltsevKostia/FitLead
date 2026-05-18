using FitLead.Domain.Messenger.VideoReports;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IVideoReportRepository
    {
        Task AddAsync(
            VideoReport videoReport,
            CancellationToken cancellationToken);

        Task<VideoReport?> GetByIdAndChatIdAsync(
            Guid reportId,
            Guid chatId,
            CancellationToken cancellationToken);
    }
}
