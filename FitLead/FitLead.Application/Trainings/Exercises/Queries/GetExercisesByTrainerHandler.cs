using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Identity;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Queries
{
    public sealed class GetExercisesByTrainerHandler
    : IRequestHandler<GetExercisesByTrainerQuery, Result<IReadOnlyList<ExerciseDto>>>
    {
        private readonly IUserContext _user;
        private readonly IExerciseReadRepository _repository;

        public GetExercisesByTrainerHandler(IUserContext user, IExerciseReadRepository repository)
        {
            _user = user;
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<ExerciseDto>>> Handle(
            GetExercisesByTrainerQuery request,
            CancellationToken cancellationToken)
        {
            if (!IsValidSource(request.Source))
            {
                return Result<IReadOnlyList<ExerciseDto>>.Failure(
                    Error.Validation(
                        "exercise.list.source_invalid",
                        "Exercise list source is invalid."));
            }

            var exercises = await _repository.GetVisibleForTrainerAsync(
                _user.UserId,
                request.Source,
                cancellationToken);
            return Result<IReadOnlyList<ExerciseDto>>.Success(exercises);
        }
        private static bool IsValidSource(ExerciseListSource source)
        {
            return source is
                ExerciseListSource.All or
                ExerciseListSource.Platform or
                ExerciseListSource.My;
        }
    }
}
