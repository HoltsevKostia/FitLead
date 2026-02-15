using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Queries
{
    public sealed record GetExercisesByTrainerQuery(

    ) : IRequest<Result<IReadOnlyList<ExerciseDto>>>;
}
