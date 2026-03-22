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
    public sealed class DeleteExerciseHandler
    : IRequestHandler<DeleteExerciseCommand, Result>
    {
        private readonly IExerciseLoader _exerciseLoader;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExerciseReadRepository _exerciseReadRepository;
        private readonly IDeletionConfirmationTokenService _tokenService;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteExerciseHandler(
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

        public async Task<Result> Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
        {
            var exerciseResult = await _exerciseLoader.GetOwnedOrNotFoundAsync(
                request.ExerciseId,
                cancellationToken);

            if (exerciseResult.IsFailure)
                return Result.Failure(exerciseResult.Error);

            var exercise = exerciseResult.Value;

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
