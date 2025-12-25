using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using MediatR;


namespace FitLead.Application.Trainings.Commands.AddExerciseToWorkout
{
    public sealed class AddExerciseToWorkoutHandler
    : IRequestHandler<AddExerciseToWorkoutCommand, Result>
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

        public async Task<Result> Handle(
            AddExerciseToWorkoutCommand request,
            CancellationToken cancellationToken)
        {
            var workout = await _repository.GetByIdAsync(
                request.WorkoutId,
                cancellationToken);

            if (workout is null)
                return Result.Failure("Workout not found");

            var exerciseExists = await _exerciseRepository.ExistsAsync(
                request.ExerciseId,
                cancellationToken);

            if (!exerciseExists)
                return Result.Failure("Exercise not found");

            workout.AddExercise(
                request.ExerciseId,
                request.Repetitions,
                request.Sets,
                request.RestSeconds);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
