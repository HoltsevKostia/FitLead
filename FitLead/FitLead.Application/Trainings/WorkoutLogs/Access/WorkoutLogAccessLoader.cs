using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common.Time;
using FitLead.Application.Users.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Users;

namespace FitLead.Application.Trainings.WorkoutLogs.Access
{
    public sealed class WorkoutLogAccessLoader : IWorkoutLogAccessLoader
    {
        private readonly ICurrentUserLoader _currentUserLoader;
        private readonly IWorkoutLogAccessRepository _accessRepository;
        private readonly IClock _clock;

        public WorkoutLogAccessLoader(
            ICurrentUserLoader currentUserLoader,
            IWorkoutLogAccessRepository accessRepository,
            IClock clock)
        {
            _currentUserLoader = currentUserLoader;
            _accessRepository = accessRepository;
            _clock = clock;
        }

        public async Task<Result<WorkoutLogAccessContext>> GetForCurrentClientOrNotFoundAsync(
            Guid assignmentId,
            Guid trainingProgramWorkoutId,
            CancellationToken cancellationToken)
        {
            var currentUserResult = await _currentUserLoader.GetCurrentOrNotFoundAsync(cancellationToken);
            if (currentUserResult.IsFailure)
            {
                return Result<WorkoutLogAccessContext>.Failure(currentUserResult.Error);
            }

            if (currentUserResult.Value.Role != UserRole.Client)
            {
                return Result<WorkoutLogAccessContext>.Failure(
                    Error.Forbidden("client.required", "User is not a client"));
            }

            var accessContext = await _accessRepository.GetAccessibleForClientAsync(
                assignmentId,
                trainingProgramWorkoutId,
                currentUserResult.Value.Id,
                _clock.UtcNow,
                cancellationToken);

            if (accessContext is null)
            {
                return Result<WorkoutLogAccessContext>.Failure(
                    Error.NotFound("workout_log.assignment_workout_not_found", "Assignment workout not found"));
            }

            return Result<WorkoutLogAccessContext>.Success(accessContext);
        }
    }
}
