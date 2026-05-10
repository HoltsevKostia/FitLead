using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Identity;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Queries
{
    public sealed class GetExerciseByIdHandler
        : IRequestHandler<GetExerciseByIdQuery, Result<ExerciseDto>>
    {
        private readonly IUserContext _user;
        private readonly IExerciseReadRepository _repository;

        public GetExerciseByIdHandler(
            IUserContext user,
            IExerciseReadRepository repository)
        {
            _user = user;
            _repository = repository;
        }

        public async Task<Result<ExerciseDto>> Handle(
            GetExerciseByIdQuery request,
            CancellationToken cancellationToken)
        {
            var exercise = await _repository.GetVisibleByIdForTrainerAsync(
                request.ExerciseId,
                _user.UserId,
                cancellationToken);

            if (exercise is null)
            {
                return Result<ExerciseDto>.Failure(
                    Error.NotFound("exercise.not_found", "Exercise not found"));
            }

            return Result<ExerciseDto>.Success(exercise);
        }
    }
}
