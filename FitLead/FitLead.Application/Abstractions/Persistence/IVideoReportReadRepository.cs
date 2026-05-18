using FitLead.Application.Messenger.VideoReports.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IVideoReportReadRepository
    {
        Task<VideoReportDetailsDto?> GetDetailsAsync(
            Guid chatId,
            Guid reportId,
            CancellationToken cancellationToken);
    }
}
