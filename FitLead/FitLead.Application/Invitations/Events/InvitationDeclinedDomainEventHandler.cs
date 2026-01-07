using MediatR;


namespace FitLead.Application.Invitations.Events
{
    public sealed class InvitationDeclinedDomainEventHandler
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
