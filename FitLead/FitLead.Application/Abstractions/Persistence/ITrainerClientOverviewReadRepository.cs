using FitLead.Application.Users.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainerClientOverviewReadRepository
    {
        Task<TrainerClientOverviewSummaryDto> GetOverviewSummaryAsync(
            Guid trainerId,
            Guid clientId,
            DateTime utcNow,
            CancellationToken cancellationToken);
    }
}
