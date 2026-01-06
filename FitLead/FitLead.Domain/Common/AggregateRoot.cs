    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Domain.Common
{
    public abstract class AggregateRoot<TId> : Entity<TId>
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

        protected void RaiseDomainEvent(IDomainEvent domainEvent)
            => _domainEvents.Add(domainEvent);

        public void ClearDomainEvents()
            => _domainEvents.Clear();
    }
}
