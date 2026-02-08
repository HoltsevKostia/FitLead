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
    public sealed class DeleteWorkoutHandler
    : IRequestHandler<DeleteWorkoutCommand, Result>
    {
        private readonly IUserContext _user;
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IWorkoutReadRepository _workoutReadRepository;
        private readonly IDeletionConfirmationTokenService _tokenService;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteWorkoutHandler(
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
            DeleteWorkoutCommand request,
            CancellationToken cancellationToken)
        {
            var workout = await _workoutRepository.GetByIdAsync(
                request.WorkoutId,
                cancellationToken);

            if (workout is null)
                return Result.Failure(Error.NotFound("workout.not_found", "Workout not found"));

            if (workout.TrainerId != _user.UserId)
                return Result.Failure(Error.Forbidden("workout.forbidden", "Forbidden"));

            var usageCount = await _workoutReadRepository.GetUsageCountAsync(
                request.WorkoutId,
                cancellationToken);

            if (usageCount > 0)
            {
                var token = _tokenService.IssueToken(
                    DeletionScope.Workout,
                    request.WorkoutId,
                    usageCount,
                    _clock.UtcNow);

                var metadata = new Dictionary<string, object?>
                {
                    ["usage"] = new { trainingProgramWorkoutCount = usageCount },
                    ["confirmationToken"] = token
                };

                return Result.Failure(Error.Conflict(
                    "workout.in_use",
                    "Workout is used in training programs",
                    metadata));
            }

            _workoutRepository.Remove(workout);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
