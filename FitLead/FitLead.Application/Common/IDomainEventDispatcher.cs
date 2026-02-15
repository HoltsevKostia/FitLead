using FitLead.Common.Domain;

namespace FitLead.Application.Common
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(
            IReadOnlyCollection<IDomainEvent> domainEvents,
            CancellationToken cancellationToken);
    }
}
