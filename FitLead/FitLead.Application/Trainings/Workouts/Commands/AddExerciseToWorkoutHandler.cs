using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.Workouts.Commands
{
    public sealed class AddExerciseToWorkoutHandler
    : IRequestHandler<AddExerciseToWorkoutCommand, Result<Guid>>
    {
        private readonly IWorkoutRepository _repository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddExerciseToWorkoutHandler(
            IWorkoutRepository repository,
            IExerciseRepository exerciseRepository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _exerciseRepository = exerciseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            AddExerciseToWorkoutCommand request,
            CancellationToken cancellationToken)
        {
            var workout = await _repository.GetByIdAsync(
                request.WorkoutId,
                cancellationToken);

            if (workout is null)
                return Result<Guid>.Failure(Error.NotFound("workout.not_found", "Workout not found"));

            var exerciseExists = await _exerciseRepository.ExistsAsync(
                request.ExerciseId,
                cancellationToken);

            if (!exerciseExists)
                return Result<Guid>.Failure(Error.NotFound("exercise.not_found", "Exercise not found"));

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
    }
}
