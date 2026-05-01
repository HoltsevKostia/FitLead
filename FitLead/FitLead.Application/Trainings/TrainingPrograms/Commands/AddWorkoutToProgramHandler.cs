using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Modules.Workouts;
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
        private readonly IWorkoutsModule _workoutsModule;
        private readonly IUnitOfWork _unitOfWork;

        public AddWorkoutToProgramHandler(
            ITrainingProgramLoader programLoader,
            IWorkoutsModule workoutsModule,
            IUnitOfWork unitOfWork)
        {
            _programLoader = programLoader;
            _workoutsModule = workoutsModule;
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

            var workout = await _workoutsModule.GetByIdAsync(
                request.WorkoutId,
                cancellationToken);

            if (workout is null)
                return Result.Failure(Error.NotFound("workout.not_found", "Workout not found"));

            if (workout.TrainerId != program.TrainerId)
                return Result.Failure(Error.Forbidden("workout.forbidden", "Workout does not belong to the same trainer as the program"));

            var addResult = program.AddWorkout(request.WorkoutId);
            if (addResult.IsFailure)
                return addResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
