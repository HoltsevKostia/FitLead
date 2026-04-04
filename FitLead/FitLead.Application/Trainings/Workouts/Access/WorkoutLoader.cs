using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Identity;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Trainings;

namespace FitLead.Application.Trainings.Workouts.Access
{
    public sealed class WorkoutLoader : IWorkoutLoader
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUserContext _userContext;

        public WorkoutLoader(
            IWorkoutRepository workoutRepository,
            IUserContext userContext)
        {
            _workoutRepository = workoutRepository;
            _userContext = userContext;
        }

        public async Task<Result<Workout>> GetOwnedOrNotFoundAsync(
            Guid workoutId,
            CancellationToken cancellationToken)
        {
            var currentUserId = _userContext.UserIdOrNull;
            if (!currentUserId.HasValue)
            {
                return Result<Workout>.Failure(
                    Error.Unauthorized("auth.user_missing", "Current user is missing"));
            }

            var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
            if (workout is null || workout.TrainerId != currentUserId.Value)
            {
                return Result<Workout>.Failure(
                    Error.NotFound("workout.not_found", "Workout not found"));
            }

            return Result<Workout>.Success(workout);
        }
    }
}
