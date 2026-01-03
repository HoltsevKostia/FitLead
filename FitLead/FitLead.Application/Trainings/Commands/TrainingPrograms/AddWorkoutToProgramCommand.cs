using FitLead.Application.Common;
using MediatR;


namespace FitLead.Application.Trainings.Commands.TrainingPrograms
{
    public sealed record AddWorkoutToProgramCommand(
        Guid ProgramId,
        Guid WorkoutId
    ) : IRequest<Result>;
}
