
using FitLead.Domain.Common;

namespace FitLead.Application.Common
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(
            IReadOnlyCollection<IDomainEvent> domainEvents,
            CancellationToken cancellationToken);
    }
}
