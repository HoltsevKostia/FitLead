using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public sealed record AddWorkoutToProgramCommand(
        Guid ProgramId,
        Guid WorkoutId,
        int WeekNumber,
        int DayNumber
    ) : IRequest<Result>;
}
