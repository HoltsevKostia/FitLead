using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using MediatR;
using FitLead.Application.Identity;

namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed class UpdateWorkoutExerciseHandler
    : IRequestHandler<UpdateWorkoutExerciseCommand, Result>
    {
        private readonly IUserContext _user;
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateWorkoutExerciseHandler(
            IUserContext user,
            IWorkoutRepository workoutRepository,
            IUnitOfWork unitOfWork)
        {
            _user = user;
            _workoutRepository = workoutRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateWorkoutExerciseCommand request, CancellationToken cancellationToken)
        {
            var workout = await _workoutRepository.GetByIdAsync(
                request.WorkoutId,
                cancellationToken);

            if (workout is null)
                return Result.Failure(Error.NotFound("workout.not_found", "Workout not found"));

            if (workout.TrainerId != _user.UserId)
                return Result.Failure(Error.Forbidden("workout.forbidden", "Forbidden"));

            var updateResult = workout.UpdateExercise(
                request.WorkoutExerciseId,
                request.Repetitions,
                request.Sets,
                request.RestSeconds);
            if (updateResult.IsFailure)
                return updateResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
