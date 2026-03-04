using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Identity;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Workouts.Queries
{
    public sealed class GetWorkoutsByTrainerHandler
    : IRequestHandler<GetWorkoutsByTrainerQuery, Result<IReadOnlyList<WorkoutDto>>>
    {
        private readonly IUserContext _user;
        private readonly IWorkoutReadRepository _repository;

        public GetWorkoutsByTrainerHandler(IUserContext user, IWorkoutReadRepository repository)
        {
            _user = user;
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<WorkoutDto>>> Handle(
            GetWorkoutsByTrainerQuery request,
            CancellationToken cancellationToken)
        {
            var workouts = await _repository.GetByTrainerIdAsync(
                _user.UserId,
                cancellationToken);
            return Result<IReadOnlyList<WorkoutDto>>.Success(workouts);
        }
    }
}
