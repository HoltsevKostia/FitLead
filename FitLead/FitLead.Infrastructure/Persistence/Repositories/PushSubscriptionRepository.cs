using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Notifications.PushSubscriptions;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Repositories
{
    public sealed class PushSubscriptionRepository : IPushSubscriptionRepository
    {
        private readonly FitLeadDbContext _context;

        public PushSubscriptionRepository(FitLeadDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            PushSubscription subscription,
            CancellationToken cancellationToken)
        {
            await _context.PushSubscriptions.AddAsync(subscription, cancellationToken);
        }

        public async Task<PushSubscription?> GetByEndpointAsync(
            string endpoint,
            CancellationToken cancellationToken)
        {
            return await _context.PushSubscriptions
                .FirstOrDefaultAsync(
                    subscription => subscription.Endpoint == endpoint,
                    cancellationToken);
        }
    }
}
