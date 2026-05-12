using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public sealed record MoveWorkoutEntryCommand(
        Guid ProgramId,
        Guid TrainingProgramWorkoutId,
        int TargetWeekNumber,
        int TargetDayNumber,
        int TargetOrderInDay
    ) : IRequest<Result>;
}
