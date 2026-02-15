using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed record RenameWorkoutCommand(
        Guid WorkoutId,
        string Name
    ) : IRequest<Result>;
}
