using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Deletion;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Application.Common.Time;
using MediatR;
using FitLead.Application.Trainings.Exercises.Access;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed class ConfirmDeleteExerciseHandler
    : IRequestHandler<ConfirmDeleteExerciseCommand, Result>
    {
        private readonly IExerciseLoader _exerciseLoader;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExerciseReadRepository _exerciseReadRepository;
        private readonly IDeletionConfirmationTokenService _tokenService;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public ConfirmDeleteExerciseHandler(
            IExerciseLoader exerciseLoader,
            IExerciseRepository exerciseRepository,
            IExerciseReadRepository exerciseReadRepository,
            IDeletionConfirmationTokenService tokenService,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _exerciseLoader = exerciseLoader;
            _exerciseRepository = exerciseRepository;
            _exerciseReadRepository = exerciseReadRepository;
            _tokenService = tokenService;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            ConfirmDeleteExerciseCommand request,
            CancellationToken cancellationToken)
        {
            var exerciseResult = await _exerciseLoader.GetOwnedOrNotFoundAsync(
                request.ExerciseId,
                cancellationToken);

            if (exerciseResult.IsFailure)
                return Result.Failure(exerciseResult.Error);

            if (!_tokenService.TryValidateToken(
                    request.Token,
                    DeletionScope.Exercise,
                    request.ExerciseId,
                    _clock.UtcNow,
                    out var payload))
            {
                return Result.Failure(Error.Validation(
                    "exercise.delete.token_invalid",
                    "Invalid or expired deletion token"));
            }

            var usageCount = await _exerciseReadRepository.GetUsageCountAsync(
                request.ExerciseId,
                cancellationToken);

            if (usageCount > 0 && usageCount != payload.UsageCount)
            {
                var newToken = _tokenService.IssueToken(
                    DeletionScope.Exercise,
                    request.ExerciseId,
                    usageCount,
                    _clock.UtcNow);

                var metadata = new Dictionary<string, object?>
                {
                    ["usage"] = new { workoutExerciseCount = usageCount },
                    ["confirmationToken"] = newToken
                };

                return Result.Failure(Error.Conflict(
                    "exercise.in_use",
                    "Exercise is used in workouts",
                    metadata));
            }

            if (usageCount > 0)
            {
                await _exerciseRepository.DeleteWorkoutExercisesByExerciseIdAsync(
                    request.ExerciseId,
                    cancellationToken);
            }

            var exercise = exerciseResult.Value;
            _exerciseRepository.Remove(exercise);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
