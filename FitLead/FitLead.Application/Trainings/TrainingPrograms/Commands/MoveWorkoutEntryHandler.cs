using FitLead.Application.Common;
using FitLead.Application.Trainings.TrainingPrograms.Access;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingPrograms.Commands
{
    public sealed class MoveWorkoutEntryHandler
        : IRequestHandler<MoveWorkoutEntryCommand, Result>
    {
        private readonly ITrainingProgramLoader _programLoader;
        private readonly IUnitOfWork _unitOfWork;

        public MoveWorkoutEntryHandler(
            ITrainingProgramLoader programLoader,
            IUnitOfWork unitOfWork)
        {
            _programLoader = programLoader;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            MoveWorkoutEntryCommand request,
            CancellationToken cancellationToken)
        {
            var programResult = await _programLoader.GetOwnedOrNotFoundAsync(
                request.ProgramId,
                cancellationToken);

            if (programResult.IsFailure)
                return Result.Failure(programResult.Error);

            var program = programResult.Value;
            var moveResult = program.MoveWorkoutEntry(
                request.TrainingProgramWorkoutId,
                request.TargetWeekNumber,
                request.TargetDayNumber,
                request.TargetOrderInDay);

            if (moveResult.IsFailure)
                return moveResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
