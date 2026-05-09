using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Modules.Exercises;
using FitLead.Application.Trainings.Workouts.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Trainings;
using MediatR;

namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed class AddExerciseToWorkoutHandler
    : IRequestHandler<AddExerciseToWorkoutCommand, Result<Guid>>
    {
        private readonly IWorkoutLoader _workoutLoader;
        private readonly IExercisesModule _exercisesModule;
        private readonly IUnitOfWork _unitOfWork;

        public AddExerciseToWorkoutHandler(
            IWorkoutLoader workoutLoader,
            IExercisesModule exercisesModule,
            IUnitOfWork unitOfWork)
        {
            _workoutLoader = workoutLoader;
            _exercisesModule = exercisesModule;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            AddExerciseToWorkoutCommand request,
            CancellationToken cancellationToken)
        {
            var workoutResult = await _workoutLoader.GetOwnedOrNotFoundAsync(
                request.WorkoutId,
                cancellationToken);

            if (workoutResult.IsFailure)
                return Result<Guid>.Failure(workoutResult.Error);

            var exercise = await _exercisesModule.GetByIdAsync(
                request.ExerciseId,
                cancellationToken);

            if (exercise is null || !IsExerciseAvailableForWorkout(exercise, workoutResult.Value.TrainerId))
                return Result<Guid>.Failure(Error.NotFound("exercise.not_found", "Exercise not found"));

            var workout = workoutResult.Value;
            var addResult = workout.AddExercise(
                request.ExerciseId,
                request.Repetitions,
                request.Sets,
                request.RestSeconds);
            if (addResult.IsFailure)
                return addResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(addResult.Value);
        }

        private static bool IsExerciseAvailableForWorkout(
            ExerciseModuleDescriptor exercise,
            Guid trainerId)
        {
            return exercise.Source == ExerciseSource.Platform
                || (exercise.Source == ExerciseSource.Trainer && exercise.OwnerTrainerId == trainerId);
        }
    }
}
