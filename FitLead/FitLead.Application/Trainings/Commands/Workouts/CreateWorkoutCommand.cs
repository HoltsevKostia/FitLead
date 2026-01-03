using FitLead.Application.Common;
using MediatR;



namespace FitLead.Application.Trainings.Commands.Workouts
{
    public sealed record CreateWorkoutCommand(
        Guid TrainerId,
        string Name
    ) : IRequest<Result<Guid>>;
}
