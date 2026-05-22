using FitLead.Domain.Notifications.PushSubscriptions;

namespace FitLead.Application.Abstractions.Persistence
{
    public interface IPushSubscriptionRepository
    {
        Task AddAsync(
            PushSubscription subscription,
            CancellationToken cancellationToken);

        Task<PushSubscription?> GetByEndpointAsync(
            string endpoint,
            CancellationToken cancellationToken);

        Task<PushSubscription?> GetByEndpointForUserAsync(
            string endpoint,
            Guid userId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<PushSubscription>> GetActiveByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken);
    }
}
