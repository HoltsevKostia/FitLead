using FitLead.Application.Common.Results;
using FitLead.Application.Trainings.Workouts.Queries;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Queries
{
    public sealed record GetWorkoutsByProgramIdQuery(
        Guid ProgramId
    ) : IRequest<Result<IReadOnlyList<WorkoutDto>>>;
}
