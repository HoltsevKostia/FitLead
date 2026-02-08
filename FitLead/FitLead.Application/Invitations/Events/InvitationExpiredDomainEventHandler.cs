using MediatR;

namespace FitLead.Application.Invitations.Events
{
    public sealed class InvitationExpiredDomainEventHandler 
        : INotificationHandler<InvitationDeclinedNotification>
    {
        public Task Handle(
            InvitationDeclinedNotification notification,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
