using FitLead.Application.Common;
using FitLead.Domain.Common;

namespace FitLead.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FitLeadDbContext _context;
        private readonly IDomainEventDispatcher _dispatcher;

        public UnitOfWork(FitLeadDbContext context, IDomainEventDispatcher dispatcher)
        {
            _context = context;
            _dispatcher = dispatcher;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var domainEvents = _context.ChangeTracker
            .Entries<AggregateRoot<Guid>>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

            await _context.SaveChangesAsync(cancellationToken);

            foreach (var entry in _context.ChangeTracker.Entries<AggregateRoot<Guid>>())
            {
                entry.Entity.ClearDomainEvents();
            }

            await _dispatcher.DispatchAsync(domainEvents, cancellationToken);
        }
    }
}
