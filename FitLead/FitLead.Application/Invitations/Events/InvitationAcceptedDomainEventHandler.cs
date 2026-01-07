using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using MediatR;

namespace FitLead.Application.Invitations.EventHandlers
{
    public sealed class InvitationAcceptedNotificationHandler
    : INotificationHandler<InvitationAcceptedNotification>
    {
        private readonly ITrainerClientRepository _trainerClientRepository;
        private readonly IUnitOfWork _unitOfWork;

        public InvitationAcceptedNotificationHandler(
            ITrainerClientRepository trainerClientRepository,
            IUnitOfWork unitOfWork)
        {
            _trainerClientRepository = trainerClientRepository;
            _unitOfWork = unitOfWork;
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

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
