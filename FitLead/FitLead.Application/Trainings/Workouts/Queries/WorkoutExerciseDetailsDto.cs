using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Workouts.Queries
{
    public sealed record WorkoutExerciseDetailsDto(
        Guid WorkoutExerciseId,
        Guid ExerciseId,
        string ExerciseName,
        string ExerciseDescription,
        string? ExerciseMediaUrl,
        int Repetitions,
        int Sets,
        int RestSeconds
    );
}
