using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed record AddExerciseToWorkoutCommand(
        Guid WorkoutId,
        Guid ExerciseId,
        int Repetitions,
        int Sets,
        int RestSeconds
    ) : IRequest<Result<Guid>>;
}
