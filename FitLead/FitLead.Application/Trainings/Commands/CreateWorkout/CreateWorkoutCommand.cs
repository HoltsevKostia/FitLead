using FitLead.Application.Common;
using MediatR;



namespace FitLead.Application.Trainings.Commands.CreateWorkout
{
    public sealed record CreateWorkoutCommand(
        Guid TrainerId,
        string Name
    ) : IRequest<Result<Guid>>;
}
