using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.TrainingPrograms.Queries
{
    public sealed record GetTrainingProgramsByTrainerIdQuery(
        
    ) : IRequest<IReadOnlyList<TrainingProgramDto>>;
}
