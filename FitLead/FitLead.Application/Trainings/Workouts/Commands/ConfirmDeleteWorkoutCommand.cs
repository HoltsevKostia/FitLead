using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed record ConfirmDeleteWorkoutCommand(
        Guid WorkoutId,
        string Token
    ) : IRequest<Result>;
}
