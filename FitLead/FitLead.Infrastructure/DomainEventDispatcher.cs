using FitLead.Application.Common;
using FitLead.Application.Invitations.EventHandlers;
using FitLead.Domain.Common;
using FitLead.Domain.Invitations.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Infrastructure
{
    public sealed class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IMediator _mediator;

        private readonly Dictionary<Type, Func<IDomainEvent, CancellationToken, Task>> _handlers;

        public DomainEventDispatcher(IMediator mediator)
        {
            _mediator = mediator;

            _handlers = new Dictionary<Type, Func<IDomainEvent, CancellationToken, Task>>
        {
            {
                typeof(InvitationAcceptedDomainEvent),
                async (domainEvent, ct) =>
                {
                    var e = (InvitationAcceptedDomainEvent)domainEvent;

                    await _mediator.Publish(
                        new InvitationAcceptedNotification(
                            e.InvitationId,
                            e.TrainerId,
                            e.ClientId),
                        ct);
                }
            }
        };
        }

        public async Task DispatchAsync(
            IReadOnlyCollection<IDomainEvent> domainEvents,
            CancellationToken cancellationToken)
        {
            foreach (var domainEvent in domainEvents)
            {
                var type = domainEvent.GetType();

                if (_handlers.TryGetValue(type, out var handler))
                {
                    await handler(domainEvent, cancellationToken);
                }
            }
        }
    }

}
