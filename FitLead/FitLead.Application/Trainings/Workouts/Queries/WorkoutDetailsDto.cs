using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Workouts.Queries
{
    public sealed record WorkoutDetailsDto(
        Guid Id,
        Guid TrainerId,
        string Name,
        IReadOnlyList<WorkoutExerciseDetailsDto> Exercises
    );
}
