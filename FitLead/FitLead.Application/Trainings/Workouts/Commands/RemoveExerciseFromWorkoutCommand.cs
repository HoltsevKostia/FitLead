using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed record RemoveExerciseFromWorkoutCommand(
        Guid WorkoutId,
        Guid WorkoutExerciseId
    ) : IRequest<Result>;
}
