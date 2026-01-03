using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Commands.TrainingPrograms
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
                return Result.Failure("Training program not found");

            var workoutExists = await _workoutRepository.ExistsAsync(
                request.WorkoutId,
                cancellationToken);

            if (!workoutExists)
                return Result.Failure("Workout not found");

            program.AddWorkout(request.WorkoutId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
