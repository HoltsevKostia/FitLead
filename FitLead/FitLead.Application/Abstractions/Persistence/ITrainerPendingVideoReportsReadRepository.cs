using FitLead.Application.TrainerVideoReports.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainerPendingVideoReportsReadRepository
    {
        Task<IReadOnlyList<TrainerPendingVideoReportDto>> GetPendingAsync(
            Guid trainerId,
            CancellationToken cancellationToken);
    }
}
