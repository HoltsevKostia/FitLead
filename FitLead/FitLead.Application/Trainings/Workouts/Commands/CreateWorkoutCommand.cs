using FitLead.Application.Common.Results;
using MediatR;



namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed record CreateWorkoutCommand(
        Guid TrainerId,
        string Name
    ) : IRequest<Result<Guid>>;
}
