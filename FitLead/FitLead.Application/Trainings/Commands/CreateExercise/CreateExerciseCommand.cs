using FitLead.Application.Common;
using MediatR;

namespace FitLead.Application.Trainings.Commands.CreateExercise
{
    public sealed record CreateExerciseCommand(
        Guid TrainerId,
        string Name,
        string Description,
        string? MediaUrl
    ) : IRequest<Result<Guid>>;
}
