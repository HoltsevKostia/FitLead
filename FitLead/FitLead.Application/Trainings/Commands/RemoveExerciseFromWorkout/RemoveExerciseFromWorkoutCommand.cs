using FitLead.Application.Common;
using MediatR;

namespace FitLead.Application.Trainings.Commands.RemoveExerciseFromWorkout
{
    public sealed record RemoveExerciseFromWorkoutCommand(
        Guid WorkoutId,
        Guid WorkoutExerciseId
    ) : IRequest<Result>;
}
