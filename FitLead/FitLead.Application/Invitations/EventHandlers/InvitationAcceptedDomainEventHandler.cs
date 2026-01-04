using FitLead.Application.Abstractions.Persistence;
using FitLead.Domain.Invitations.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Invitations.EventHandlers
{
    public sealed class InvitationAcceptedDomainEventHandler
    : INotificationHandler<InvitationAcceptedDomainEvent>
    {
        private readonly ITrainerClientRepository _trainerClientRepository;

        public InvitationAcceptedDomainEventHandler(
            ITrainerClientRepository trainerClientRepository)
        {
            _trainerClientRepository = trainerClientRepository;
        }

        public async Task Handle(
            InvitationAcceptedDomainEvent notification,
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
