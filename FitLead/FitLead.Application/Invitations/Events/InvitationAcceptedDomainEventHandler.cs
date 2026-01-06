using FitLead.Application.Abstractions.Persistence;
using MediatR;

namespace FitLead.Application.Invitations.EventHandlers
{
    public sealed class InvitationAcceptedNotificationHandler
    : INotificationHandler<InvitationAcceptedNotification>
    {
        private readonly ITrainerClientRepository _trainerClientRepository;

        public InvitationAcceptedNotificationHandler(
            ITrainerClientRepository trainerClientRepository)
        {
            _trainerClientRepository = trainerClientRepository;
        }

        public async Task Handle(
            InvitationAcceptedNotification notification,
            CancellationToken cancellationToken)
        {
            var exists = await _trainerClientRepository.ExistsAsync(
                notification.TrainerId,
                notification.ClientId,
                cancellationToken);

            if (exists)
                return;

            await _trainerClientRepository.AddAsync(
                notification.TrainerId,
                notification.ClientId,
                cancellationToken);
        }
    }

}
