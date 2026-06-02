using FitLead.Application.Users.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainerClientVideoReportsReadRepository
    {
        Task<IReadOnlyList<TrainerClientVideoReportDto>> GetRecentVideoReportsAsync(
            Guid trainerId,
            Guid clientId,
            int limit,
            CancellationToken cancellationToken);
    }
}
