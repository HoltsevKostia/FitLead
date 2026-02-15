using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed record CreateExerciseCommand(
        string Name,
        string Description,
        string? MediaUrl
    ) : IRequest<Result<Guid>>;
}
