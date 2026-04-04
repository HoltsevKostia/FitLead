using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Deletion;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Application.Common.Time;
using MediatR;
using FitLead.Application.Trainings.Workouts.Access;

namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed class ConfirmDeleteWorkoutHandler
    : IRequestHandler<ConfirmDeleteWorkoutCommand, Result>
    {
        private readonly IWorkoutLoader _workoutLoader;
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IWorkoutReadRepository _workoutReadRepository;
        private readonly IDeletionConfirmationTokenService _tokenService;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public ConfirmDeleteWorkoutHandler(
            IWorkoutLoader workoutLoader,
            IWorkoutRepository workoutRepository,
            IWorkoutReadRepository workoutReadRepository,
            IDeletionConfirmationTokenService tokenService,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _workoutLoader = workoutLoader;
            _workoutRepository = workoutRepository;
            _workoutReadRepository = workoutReadRepository;
            _tokenService = tokenService;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            ConfirmDeleteWorkoutCommand request,
            CancellationToken cancellationToken)
        {
            var workoutResult = await _workoutLoader.GetOwnedOrNotFoundAsync(
                request.WorkoutId,
                cancellationToken);

            if (workoutResult.IsFailure)
                return Result.Failure(workoutResult.Error);

            if (!_tokenService.TryValidateToken(
                    request.Token,
                    DeletionScope.Workout,
                    request.WorkoutId,
                    _clock.UtcNow,
                    out var payload))
            {
                return Result.Failure(Error.Validation(
                    "workout.delete.token_invalid",
                    "Invalid or expired deletion token"));
            }

            var usageCount = await _workoutReadRepository.GetUsageCountAsync(
                request.WorkoutId,
                cancellationToken);

            if (usageCount > 0 && usageCount != payload.UsageCount)
            {
                var newToken = _tokenService.IssueToken(
                    DeletionScope.Workout,
                    request.WorkoutId,
                    usageCount,
                    _clock.UtcNow);

                var metadata = new Dictionary<string, object?>
                {
                    ["usage"] = new { trainingProgramWorkoutCount = usageCount },
                    ["confirmationToken"] = newToken
                };

                return Result.Failure(Error.Conflict(
                    "workout.in_use",
                    "Workout is used in training programs",
                    metadata));
            }

            if (usageCount > 0)
            {
                await _workoutRepository.DeleteTrainingProgramWorkoutsByWorkoutIdAsync(
                    request.WorkoutId,
                    cancellationToken);
            }

            var workout = workoutResult.Value;
            _workoutRepository.Remove(workout);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
