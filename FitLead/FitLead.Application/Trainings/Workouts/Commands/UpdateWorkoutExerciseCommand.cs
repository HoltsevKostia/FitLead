using FitLead.Application.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed record UpdateWorkoutExerciseCommand(
        Guid WorkoutId,
        Guid WorkoutExerciseId,
        int Repetitions,
        int Sets,
        int RestSeconds
    ) : IRequest<Result>;
}
