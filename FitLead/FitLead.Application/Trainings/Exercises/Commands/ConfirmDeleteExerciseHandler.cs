using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Deletion;
using FitLead.Common.Errors;
using FitLead.Application.Common.Identity;
using FitLead.Common.Results;
using FitLead.Application.Common.Time;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed class ConfirmDeleteExerciseHandler
    : IRequestHandler<ConfirmDeleteExerciseCommand, Result>
    {
        private readonly IUserContext _user;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExerciseReadRepository _exerciseReadRepository;
        private readonly IDeletionConfirmationTokenService _tokenService;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public ConfirmDeleteExerciseHandler(
            IUserContext user,
            IExerciseRepository exerciseRepository,
            IExerciseReadRepository exerciseReadRepository,
            IDeletionConfirmationTokenService tokenService,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _user = user;
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
            var exercise = await _exerciseRepository.GetByIdAsync(
                request.ExerciseId,
                cancellationToken);

            if (exercise is null)
                return Result.Failure(Error.NotFound("exercise.not_found", "Exercise not found"));

            if (exercise.TrainerId != _user.UserId)
                return Result.Failure(Error.Forbidden("exercise.forbidden", "Forbidden"));

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

            _exerciseRepository.Remove(exercise);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
