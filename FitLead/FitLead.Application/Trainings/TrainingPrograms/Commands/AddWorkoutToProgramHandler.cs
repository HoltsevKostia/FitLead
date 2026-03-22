using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Trainings.TrainingPrograms.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public sealed class AddWorkoutToProgramHandler
    : IRequestHandler<AddWorkoutToProgramCommand, Result>
    {
        private readonly ITrainingProgramLoader _programLoader;
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddWorkoutToProgramHandler(
            ITrainingProgramLoader programLoader,
            IWorkoutRepository workoutRepository,
            IUnitOfWork unitOfWork)
        {
            _programLoader = programLoader;
            _workoutRepository = workoutRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            AddWorkoutToProgramCommand request,
            CancellationToken cancellationToken)
        {
            var programResult = await _programLoader.GetOwnedOrNotFoundAsync(
                request.ProgramId,
                cancellationToken);

            if (programResult.IsFailure)
                return Result.Failure(programResult.Error);

            var program = programResult.Value;

            var workoutExists = await _workoutRepository.ExistsAsync(
                request.WorkoutId,
                cancellationToken);

            if (!workoutExists)
                return Result.Failure(Error.NotFound("workout.not_found", "Workout not found"));

            var workoutTrainerId = await _workoutRepository.GetTrainerIdAsync(request.WorkoutId, cancellationToken);

            if (workoutTrainerId.Value != program.TrainerId)
                return Result.Failure(Error.Forbidden("workout.forbidden", "Workout does not belong to the same trainer as the program"));

            var addResult = program.AddWorkout(request.WorkoutId);
            if (addResult.IsFailure)
                return addResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
