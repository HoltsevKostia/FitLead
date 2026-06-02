using FitLead.Application.Users.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainerClientWorkoutLogsReadRepository
    {
        Task<IReadOnlyList<TrainerClientWorkoutLogDto>> GetRecentWorkoutLogsAsync(
            Guid trainerId,
            Guid clientId,
            int limit,
            CancellationToken cancellationToken);
    }
}
