using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Deletion;
using FitLead.Application.Common.Errors;
using FitLead.Application.Common.Identity;
using FitLead.Application.Common.Results;
using FitLead.Application.Common.Time;
using MediatR;

namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed class ConfirmDeleteWorkoutHandler
    : IRequestHandler<ConfirmDeleteWorkoutCommand, Result>
    {
        private readonly IUserContext _user;
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IWorkoutReadRepository _workoutReadRepository;
        private readonly IDeletionConfirmationTokenService _tokenService;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public ConfirmDeleteWorkoutHandler(
            IUserContext user,
            IWorkoutRepository workoutRepository,
            IWorkoutReadRepository workoutReadRepository,
            IDeletionConfirmationTokenService tokenService,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _user = user;
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
            var workout = await _workoutRepository.GetByIdAsync(
                request.WorkoutId,
                cancellationToken);

            if (workout is null)
                return Result.Failure(Error.NotFound("workout.not_found", "Workout not found"));

            if (workout.TrainerId != _user.UserId)
                return Result.Failure(Error.Forbidden("workout.forbidden", "Forbidden"));

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

            _workoutRepository.Remove(workout);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
