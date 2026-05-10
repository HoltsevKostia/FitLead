using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Queries
{
    public sealed record GetExercisesByTrainerQuery(
        ExerciseListSource Source = ExerciseListSource.All
    ) : IRequest<Result<IReadOnlyList<ExerciseDto>>>;
}
