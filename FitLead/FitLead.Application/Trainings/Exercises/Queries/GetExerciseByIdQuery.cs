using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Queries
{
    public sealed record GetExerciseByIdQuery(Guid ExerciseId)
        : IRequest<Result<ExerciseDto>>;
}
