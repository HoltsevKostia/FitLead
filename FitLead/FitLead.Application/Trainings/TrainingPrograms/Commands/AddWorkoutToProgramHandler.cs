using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Errors;
using FitLead.Application.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public sealed class AddWorkoutToProgramHandler
    : IRequestHandler<AddWorkoutToProgramCommand, Result>
    {
        private readonly ITrainingProgramRepository _programRepository;
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddWorkoutToProgramHandler(
            ITrainingProgramRepository programRepository,
            IWorkoutRepository workoutRepository,
            IUnitOfWork unitOfWork)
        {
            _programRepository = programRepository;
            _workoutRepository = workoutRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            AddWorkoutToProgramCommand request,
            CancellationToken cancellationToken)
        {
            var program = await _programRepository.GetByIdAsync(
                request.ProgramId,
                cancellationToken);

            if (program is null)
                return Result.Failure(Error.NotFound("training_program.not_found", "Training program not found"));

            var workoutExists = await _workoutRepository.ExistsAsync(
                request.WorkoutId,
                cancellationToken);

            if (!workoutExists)
                return Result.Failure(Error.NotFound("workout.not_found", "Workout not found"));

            var workoutTrainerId = await _workoutRepository.GetTrainerIdAsync(request.WorkoutId, cancellationToken);

            if (workoutTrainerId.Value != program.TrainerId)
                return Result.Failure(Error.Forbidden("workout.forbidden", "Workout does not belong to the same trainer as the program"));

            program.AddWorkout(request.WorkoutId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
