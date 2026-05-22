using FitLead.Common.Results;
using FitLead.Domain.Trainings.Exercises;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed record CreateExerciseCommand(
        string Name,
        string Description,
        Guid? MediaAssetId,
        MuscleGroup? MuscleGroup,
        Equipment? Equipment
    ) : IRequest<Result<Guid>>;
}
