using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingProgramAssignments.Commands
{
    public sealed record RevokeTrainingProgramAssignmentCommand(
        Guid TrainingProgramId,
        Guid AssignmentId
    ) : IRequest<Result>;
}
