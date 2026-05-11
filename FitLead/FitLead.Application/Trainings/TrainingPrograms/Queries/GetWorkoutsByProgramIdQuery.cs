using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Queries
{
    public sealed record GetWorkoutsByProgramIdQuery(
        Guid ProgramId
    ) : IRequest<Result<IReadOnlyList<TrainingProgramWorkoutDto>>>;
}
