using FitLead.Application.Common;
using FitLead.Application.Trainings.TrainingPrograms.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public sealed class RemoveWorkoutFromProgramHandler
    : IRequestHandler<RemoveWorkoutFromProgramCommand, Result>
    {
        private readonly ITrainingProgramLoader _programLoader;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveWorkoutFromProgramHandler(
            ITrainingProgramLoader programLoader,
            IUnitOfWork unitOfWork)
        {
            _programLoader = programLoader;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            RemoveWorkoutFromProgramCommand request,
            CancellationToken cancellationToken)
        {
            var programResult = await _programLoader.GetOwnedOrNotFoundAsync(
                request.ProgramId,
                cancellationToken);

            if (programResult.IsFailure)
                return Result.Failure(programResult.Error);

            var program = programResult.Value;
            var removeResult = program.RemoveWorkoutEntry(request.TrainingProgramWorkoutId);
            if (removeResult.IsFailure)
                return removeResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
