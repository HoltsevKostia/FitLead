using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Trainings.TrainingPrograms.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public sealed class ReorderProgramWorkoutsHandler
    : IRequestHandler<ReorderProgramWorkoutsCommand, Result>
    {
        private readonly ITrainingProgramLoader _programLoader;
        private readonly IUnitOfWork _unitOfWork;

        public ReorderProgramWorkoutsHandler(
            ITrainingProgramLoader programLoader,
            IUnitOfWork unitOfWork)
        {
            _programLoader = programLoader;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ReorderProgramWorkoutsCommand request, CancellationToken cancellationToken)
        {
            var programResult = await _programLoader.GetOwnedOrNotFoundAsync(
                request.ProgramId,
                cancellationToken);
            if (programResult.IsFailure)
                return Result.Failure(programResult.Error);

            var program = programResult.Value;
            var reorderResult = program.ReorderWorkouts(request.OrderedWorkoutIds);
            if (reorderResult.IsFailure)
                return reorderResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }

}
