using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Trainings.TrainingPrograms.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using MediatR;

namespace FitLead.Application.Trainings.TrainingProgramAssignments.Commands
{
    public sealed class RevokeTrainingProgramAssignmentHandler
        : IRequestHandler<RevokeTrainingProgramAssignmentCommand, Result>
    {
        private readonly ITrainingProgramLoader _programLoader;
        private readonly IAssignedTrainingProgramRepository _assignmentRepository;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public RevokeTrainingProgramAssignmentHandler(
            ITrainingProgramLoader programLoader,
            IAssignedTrainingProgramRepository assignmentRepository,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _programLoader = programLoader;
            _assignmentRepository = assignmentRepository;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            RevokeTrainingProgramAssignmentCommand request,
            CancellationToken cancellationToken)
        {
            var programResult = await _programLoader.GetOwnedOrNotFoundAsync(
                request.TrainingProgramId,
                cancellationToken);
            if (programResult.IsFailure)
            {
                return Result.Failure(programResult.Error);
            }

            var assignment = await _assignmentRepository.GetByIdForProgramAndTrainerAsync(
                request.AssignmentId,
                programResult.Value.Id,
                programResult.Value.TrainerId,
                cancellationToken);
            if (assignment is null)
            {
                return Result.Failure(
                    Error.NotFound("training_program.assignment.not_found", "Assignment not found"));
            }

            var revokeResult = assignment.Revoke(_clock.UtcNow);
            if (revokeResult.IsFailure)
            {
                return revokeResult;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
