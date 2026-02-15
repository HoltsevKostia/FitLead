using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Workouts.Queries
{
    public sealed record GetWorkoutDetailsByIdQuery(Guid WorkoutId)
    : IRequest<Result<WorkoutDetailsDto>>;
}
