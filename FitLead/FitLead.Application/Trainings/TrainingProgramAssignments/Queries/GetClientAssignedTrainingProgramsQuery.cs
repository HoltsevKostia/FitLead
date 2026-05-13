using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingProgramAssignments.Queries
{
    public sealed record GetClientAssignedTrainingProgramsQuery(
    ) : IRequest<Result<IReadOnlyList<ClientAssignedTrainingProgramDto>>>;
}
