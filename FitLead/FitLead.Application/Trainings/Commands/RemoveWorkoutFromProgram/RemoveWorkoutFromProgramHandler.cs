using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.Commands.RemoveWorkoutFromProgram
{
    public sealed class RemoveWorkoutFromProgramHandler
    : IRequestHandler<RemoveWorkoutFromProgramCommand, Result>
    {
        private readonly ITrainingProgramRepository _programRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveWorkoutFromProgramHandler(
            ITrainingProgramRepository programRepository,
            IUnitOfWork unitOfWork)
        {
            _programRepository = programRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            RemoveWorkoutFromProgramCommand request,
            CancellationToken cancellationToken)
        {
            var program = await _programRepository.GetByIdAsync(
                request.ProgramId,
                cancellationToken);

            if (program is null)
                return Result.Failure("Training program not found");

            program.RemoveWorkout(request.WorkoutId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
