using FitLead.Application.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed record UpdateExerciseCommand(
        Guid ExerciseId,
        string Name,
        string Description,
        string? MediaUrl
    ) : IRequest<Result>;
}
