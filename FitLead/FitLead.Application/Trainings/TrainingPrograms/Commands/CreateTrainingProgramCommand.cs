using FitLead.Application.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public record CreateTrainingProgramCommand(
        string Title
    ) : IRequest<Result<Guid>>;
}
