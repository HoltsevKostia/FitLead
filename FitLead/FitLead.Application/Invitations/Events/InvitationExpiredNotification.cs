using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Invitations.Events
{
    public sealed record InvitationExpiredNotification(
        Guid InvitationId,
        Guid TrainerId,
        Guid ClientId
    ) : INotification;
}
