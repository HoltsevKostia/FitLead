namespace FitLead.Application.Abstractions.Persistence
{
    public interface ITrainerClientRepository
    {
        Task<bool> ExistsAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken);

        Task<Guid?> GetTrainerIdByClientIdAsync(
            Guid clientId,
            CancellationToken cancellationToken);

        Task AddAsync(
            Guid trainerId,
            Guid clientId,
            CancellationToken cancellationToken);
    }
}
