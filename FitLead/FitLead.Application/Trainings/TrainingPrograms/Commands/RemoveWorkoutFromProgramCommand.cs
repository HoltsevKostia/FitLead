using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public sealed record RemoveWorkoutFromProgramCommand(
        Guid ProgramId,
        Guid WorkoutId
    ) : IRequest<Result>;
}
