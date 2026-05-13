using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingProgramAssignments.Queries
{
    public sealed record GetClientAssignedTrainingProgramDetailsQuery(
        Guid AssignmentId
    ) : IRequest<Result<ClientAssignedTrainingProgramDetailsDto>>;
}
