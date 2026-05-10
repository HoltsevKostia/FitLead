using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Identity;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Trainings;

namespace FitLead.Application.Trainings.Exercises.Access
{
    public sealed class ExerciseLoader : IExerciseLoader
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUserContext _userContext;

        public ExerciseLoader(
            IExerciseRepository exerciseRepository,
            IUserContext userContext)
        {
            _exerciseRepository = exerciseRepository;
            _userContext = userContext;
        }

        public async Task<Result<Exercise>> GetOwnedOrNotFoundAsync(
            Guid exerciseId,
            CancellationToken cancellationToken)
        {
            var currentUserId = _userContext.UserIdOrNull;
            if (!currentUserId.HasValue)
            {
                return Result<Exercise>.Failure(
                    Error.Unauthorized("auth.user_missing", "Current user is missing"));
            }

            var exercise = await _exerciseRepository.GetByIdAsync(exerciseId, cancellationToken);
            if (exercise is null || exercise.OwnerTrainerId != currentUserId.Value)
            {
                return Result<Exercise>.Failure(
                    Error.NotFound("exercise.not_found", "Exercise not found"));
            }

            return Result<Exercise>.Success(exercise);
        }
    }
}
