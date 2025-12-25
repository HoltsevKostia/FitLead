using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Queries.Workout
{
    public sealed record GetWorkoutsByTrainerQuery(
        Guid TrainerId
    ) : IRequest<IReadOnlyList<WorkoutDto>>;
}
