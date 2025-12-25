using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using MediatR;


namespace FitLead.Application.Trainings.Commands.RemoveExerciseFromWorkout
{
    public sealed class RemoveExerciseFromWorkoutHandler
    : IRequestHandler<RemoveExerciseFromWorkoutCommand, Result>
    {
        private readonly IWorkoutRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveExerciseFromWorkoutHandler(
            IWorkoutRepository repository,
            IUnitOfWork unitOfWork)
        {
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
                return Result.Failure("Workout not found");

            workout.RemoveExercise(request.WorkoutExerciseId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
