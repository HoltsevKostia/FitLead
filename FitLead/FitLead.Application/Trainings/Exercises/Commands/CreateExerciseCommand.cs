using FitLead.Application.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed record CreateExerciseCommand(
        Guid TrainerId,
        string Name,
        string Description,
        string? MediaUrl
    ) : IRequest<Result<Guid>>;
}
