using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Invitations.Events
{
    public sealed class InvitationExpiredDomainEventHandler 
        : INotificationHandler<InvitationDeclinedNotification>
    {
        public Task Handle(
            InvitationDeclinedNotification notification,
            CancellationToken cancellationToken)
        {
            // future: notify trainer, analytics, etc.
            return Task.CompletedTask;
        }
    }
}
