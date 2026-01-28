using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Identity;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Queries
{
    public sealed class GetExercisesByTrainerHandler
    : IRequestHandler<GetExercisesByTrainerQuery, IReadOnlyList<ExerciseDto>>
    {
        private readonly IUserContext _user;
        private readonly IExerciseReadRepository _repository;

        public GetExercisesByTrainerHandler(IUserContext user, IExerciseReadRepository repository)
        {
            _user = user;
            _repository = repository;
        }

        public async Task<IReadOnlyList<ExerciseDto>> Handle(
            GetExercisesByTrainerQuery request,
            CancellationToken cancellationToken)
        {
            return await _repository.GetByTrainerIdAsync(
                _user.UserId,
                cancellationToken);
        }
    }
}
