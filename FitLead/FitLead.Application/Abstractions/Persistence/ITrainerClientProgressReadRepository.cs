using FitLead.Application.Users.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainerClientProgressReadRepository
    {
        Task<TrainerClientProgressDto> GetProgressAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken);
    }
}
