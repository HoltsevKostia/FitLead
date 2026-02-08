using FitLead.Application.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed record DeleteExerciseCommand(
        Guid ExerciseId
    ) : IRequest<Result>;
}
