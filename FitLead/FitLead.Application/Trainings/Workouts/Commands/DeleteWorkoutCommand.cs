using FitLead.Application.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed record DeleteWorkoutCommand(
        Guid WorkoutId
    ) : IRequest<Result>;
}
