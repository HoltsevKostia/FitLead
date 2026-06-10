using FitLead.Application.TrainerDashboard.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainerDashboardReadRepository
    {
        Task<TrainerDashboardSummaryDto> GetSummaryAsync(
            Guid trainerId,
            DateTime utcNow,
            CancellationToken cancellationToken);
    }
}
