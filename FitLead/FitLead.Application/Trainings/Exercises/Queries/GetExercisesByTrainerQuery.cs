using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Exercises.Queries
{
    public sealed record GetExercisesByTrainerQuery(

    ) : IRequest<IReadOnlyList<ExerciseDto>>;
}
