using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingProgramAssignments.Queries
{
    public sealed record GetTrainingProgramAssignmentsQuery(
        Guid TrainingProgramId
    ) : IRequest<Result<IReadOnlyList<TrainingProgramAssignmentDto>>>;
}
