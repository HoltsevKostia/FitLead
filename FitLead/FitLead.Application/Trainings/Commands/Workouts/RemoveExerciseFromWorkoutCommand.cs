using FitLead.Application.Common;
using MediatR;

namespace FitLead.Application.Trainings.Commands.Workouts
{
    public sealed record RemoveExerciseFromWorkoutCommand(
        Guid WorkoutId,
        Guid WorkoutExerciseId
    ) : IRequest<Result>;
}
