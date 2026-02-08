using FitLead.Application.Users.Queries;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainerClientReadRepository
    {
        Task<IReadOnlyList<TrainerClientDto>> GetClientsByTrainerIdAsync(
            Guid trainerId,
            CancellationToken cancellationToken);
    }
}
