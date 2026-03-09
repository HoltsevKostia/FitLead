using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Trainings.Workouts.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed class UpdateWorkoutExerciseHandler
    : IRequestHandler<UpdateWorkoutExerciseCommand, Result>
    {
        private readonly IWorkoutLoader _workoutLoader;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateWorkoutExerciseHandler(
            IWorkoutLoader workoutLoader,
            IUnitOfWork unitOfWork)
        {
            _workoutLoader = workoutLoader;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateWorkoutExerciseCommand request, CancellationToken cancellationToken)
        {
            var workoutResult = await _workoutLoader.GetOwnedOrNotFoundAsync(
                request.WorkoutId,
                cancellationToken);

            if (workoutResult.IsFailure)
                return Result.Failure(workoutResult.Error);

            var workout = workoutResult.Value;
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
