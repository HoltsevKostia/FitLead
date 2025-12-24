using FitLead.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Commands.CreateTrainingProgram
{
    public record CreateTrainingProgramCommand(
        Guid TrainerId,
        string Title
    ) : IRequest<Result<Guid>>;
}
