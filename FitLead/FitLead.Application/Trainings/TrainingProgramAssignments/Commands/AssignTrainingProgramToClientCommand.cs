using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingProgramAssignments.Commands
{
    public sealed record AssignTrainingProgramToClientCommand(
        Guid TrainingProgramId,
        Guid ClientId,
        DateTime? ExpiresAtUtc
    ) : IRequest<Result<AssignTrainingProgramToClientResult>>;
}
