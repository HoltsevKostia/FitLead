using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Identity;
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
            var exercises = await _repository.GetByTrainerIdAsync(
                _user.UserId,
                cancellationToken);
            return Result<IReadOnlyList<ExerciseDto>>.Success(exercises);
        }
    }
}
