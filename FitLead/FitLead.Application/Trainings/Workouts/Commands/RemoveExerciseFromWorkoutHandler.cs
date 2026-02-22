using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Common.Errors;
using FitLead.Application.Common.Identity;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed class RemoveExerciseFromWorkoutHandler
    : IRequestHandler<RemoveExerciseFromWorkoutCommand, Result>
    {
        private readonly IUserContext _user;
        private readonly IWorkoutRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveExerciseFromWorkoutHandler(
            IUserContext user,
            IWorkoutRepository repository,
            IUnitOfWork unitOfWork)
        {
            _user = user;
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            RemoveExerciseFromWorkoutCommand request,
            CancellationToken cancellationToken)
        {
            var workout = await _repository.GetByIdAsync(
                request.WorkoutId,
                cancellationToken);

            if (workout is null)
                return Result.Failure(Error.NotFound("workout.not_found", "Workout not found"));

            if (workout.TrainerId != _user.UserId)
                return Result.Failure(Error.Forbidden("workout.forbidden", "Forbidden"));

            var removeResult = workout.RemoveExercise(request.WorkoutExerciseId);
            if (removeResult.IsFailure)
                return removeResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
