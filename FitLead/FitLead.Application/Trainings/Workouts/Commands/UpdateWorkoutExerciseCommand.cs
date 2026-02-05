using FitLead.Application.Common.Results;
using MediatR;

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
