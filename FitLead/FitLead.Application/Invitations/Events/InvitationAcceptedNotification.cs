using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Invitations.EventHandlers
{
    public sealed record InvitationAcceptedNotification(
        Guid InvitationId,
        Guid TrainerId,
        Guid ClientId
    ) : INotification;
}
