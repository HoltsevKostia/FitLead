using FitLead.Common.Results;
using FitLead.Domain.Trainings;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed record UpdateExerciseCommand(
        Guid ExerciseId,
        string Name,
        string Description,
        string? MediaUrl,
        MuscleGroup? MuscleGroup,
        Equipment? Equipment
    ) : IRequest<Result>;
}
