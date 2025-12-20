using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Users.Queries
{
    public sealed record GetClientsByTrainerIdQuery(
    Guid TrainerId
) : IRequest<IReadOnlyList<TrainerClientDto>>;
}
