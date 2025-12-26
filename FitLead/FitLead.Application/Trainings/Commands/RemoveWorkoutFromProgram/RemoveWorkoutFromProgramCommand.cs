using FitLead.Application.Common;
using MediatR;


namespace FitLead.Application.Trainings.Commands.RemoveWorkoutFromProgram
{
    public sealed record RemoveWorkoutFromProgramCommand(
        Guid ProgramId,
        Guid WorkoutId
    ) : IRequest<Result>;
}
