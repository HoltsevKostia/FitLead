using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed record ConfirmDeleteExerciseCommand(
        Guid ExerciseId,
        string Token
    ) : IRequest<Result>;
}
