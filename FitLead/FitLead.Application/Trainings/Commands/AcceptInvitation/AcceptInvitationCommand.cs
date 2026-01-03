using FitLead.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Commands.AcceptInvitation
{
    public sealed record AcceptInvitationCommand(
        Guid ClientId,
        Guid InvitationId,
        DateTime Now
    ) : IRequest<Result>;
}
