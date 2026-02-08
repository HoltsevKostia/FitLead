using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Deletion;
using FitLead.Application.Common.Errors;
using FitLead.Application.Common.Identity;
using FitLead.Application.Common.Results;
using FitLead.Application.Common.Time;
using MediatR;

namespace FitLead.Application.Trainings.Exercises.Commands
{
    public sealed class DeleteExerciseHandler
    : IRequestHandler<DeleteExerciseCommand, Result>
    {
        private readonly IUserContext _user;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExerciseReadRepository _exerciseReadRepository;
        private readonly IDeletionConfirmationTokenService _tokenService;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteExerciseHandler(
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

        public async Task<Result> Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(
                request.ExerciseId,
                cancellationToken);

            if (exercise is null)
                return Result.Failure(Error.NotFound("exercise.not_found", "Exercise not found"));

            if (exercise.TrainerId != _user.UserId)
                return Result.Failure(Error.Forbidden("exercise.forbidden", "Forbidden"));

            var usageCount = await _exerciseReadRepository.GetUsageCountAsync(
                exercise.Id,
                cancellationToken);

            if (usageCount > 0)
            {
                var token = _tokenService.IssueToken(
                    DeletionScope.Exercise,
                    exercise.Id,
                    usageCount,
                    _clock.UtcNow);

                var metadata = new Dictionary<string, object?>
                {
                    ["usage"] = new { workoutExerciseCount = usageCount },
                    ["confirmationToken"] = token
                };

                return Result.Failure(Error.Conflict(
                    "exercise.in_use",
                    "Exercise is used in workouts",
                    metadata));
            }

            _exerciseRepository.Remove(exercise);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

}
