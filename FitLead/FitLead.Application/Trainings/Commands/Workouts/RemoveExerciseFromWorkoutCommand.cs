using FitLead.Application.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Commands.Workouts
{
    public sealed record RemoveExerciseFromWorkoutCommand(
        Guid WorkoutId,
        Guid WorkoutExerciseId
    ) : IRequest<Result>;
}
