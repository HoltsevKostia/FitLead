using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Queries.Exercise
{
    public sealed record GetExercisesByTrainerQuery(
        Guid TrainerId
    ) : IRequest<IReadOnlyList<ExerciseDto>>;
}
