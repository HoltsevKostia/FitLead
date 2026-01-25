using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public sealed class ReorderProgramWorkoutsHandler
    : IRequestHandler<ReorderProgramWorkoutsCommand, Result>
    {
        private readonly ITrainingProgramRepository _programRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReorderProgramWorkoutsHandler(
            ITrainingProgramRepository programRepository,
            IUnitOfWork unitOfWork)
        {
            _programRepository = programRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ReorderProgramWorkoutsCommand request, CancellationToken cancellationToken)
        {
            var program = await _programRepository.GetByIdAsync(request.ProgramId, cancellationToken);
            if (program is null)
                return Result.Failure("Training program not found");

            program.ReorderWorkouts(request.OrderedWorkoutIds);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }

}
