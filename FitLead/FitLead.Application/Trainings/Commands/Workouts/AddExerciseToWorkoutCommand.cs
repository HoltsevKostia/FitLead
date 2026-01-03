using FitLead.Application.Common;
using MediatR;


namespace FitLead.Application.Trainings.Commands.Workouts
{
    public sealed record AddExerciseToWorkoutCommand(
        Guid WorkoutId,
        Guid ExerciseId,
        int Repetitions,
        int Sets,
        int RestSeconds
    ) : IRequest<Result>;
}
