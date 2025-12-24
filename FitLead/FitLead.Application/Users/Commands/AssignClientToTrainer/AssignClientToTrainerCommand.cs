using FitLead.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Users.Commands.AssignClientToTrainer
{
    public sealed record AssignClientToTrainerCommand(
        Guid TrainerId,
        Guid ClientId
    ) : IRequest<Result>;
}
