using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Errors;
using FitLead.Application.Common.Identity;
using FitLead.Application.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Workouts.Queries
{
    public sealed class GetWorkoutDetailsByIdHandler
        : IRequestHandler<GetWorkoutDetailsByIdQuery, Result<WorkoutDetailsDto>>
    {
        private readonly IUserContext _user;
        private readonly IWorkoutReadRepository _repository;
        public GetWorkoutDetailsByIdHandler(IUserContext user, IWorkoutReadRepository workoutReadRepository) {
            _user = user;
            _repository = workoutReadRepository;
        }

        public async Task<Result<WorkoutDetailsDto>> Handle(GetWorkoutDetailsByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _repository.GetWorkoutDetailsByIdAsync(
                request.WorkoutId,
                _user.UserId,
                cancellationToken);

            if (dto is null)
                return Result<WorkoutDetailsDto>.Failure(Error.NotFound("workout.not_found", "Workout not found"));

            return Result<WorkoutDetailsDto>.Success(dto);
        }
    }
}

