using FitLead.Application.Common;
using FitLead.Common.Domain;

namespace FitLead.Infrastructure
{
    public sealed class DomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(
            IReadOnlyCollection<IDomainEvent> domainEvents,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
