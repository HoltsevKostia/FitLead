using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using MediatR;

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
                return Result.Failure(Error.NotFound("training_program.not_found", "Training program not found"));

            var reorderResult = program.ReorderWorkouts(request.OrderedWorkoutIds);
            if (reorderResult.IsFailure)
                return reorderResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }

}
