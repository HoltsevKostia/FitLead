using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Modules.Users;
using FitLead.Application.Trainings.TrainingPrograms.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Trainings.TrainingProgramAssignments;
using MediatR;

namespace FitLead.Application.Trainings.TrainingProgramAssignments.Commands
{
    public sealed class AssignTrainingProgramToClientHandler
        : IRequestHandler<AssignTrainingProgramToClientCommand, Result<AssignTrainingProgramToClientResult>>
    {
        private readonly ITrainingProgramLoader _programLoader;
        private readonly IAssignedTrainingProgramRepository _assignmentRepository;
        private readonly IUsersModule _usersModule;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public AssignTrainingProgramToClientHandler(
            ITrainingProgramLoader programLoader,
            IAssignedTrainingProgramRepository assignmentRepository,
            IUsersModule usersModule,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _programLoader = programLoader;
            _assignmentRepository = assignmentRepository;
            _usersModule = usersModule;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AssignTrainingProgramToClientResult>> Handle(
            AssignTrainingProgramToClientCommand request,
            CancellationToken cancellationToken)
        {
            if (request.ClientId == Guid.Empty)
            {
                return Result<AssignTrainingProgramToClientResult>.Failure(
                    Error.Validation("training_program.assignment.client_id_required", "ClientId is required"));
            }

            var programResult = await _programLoader.GetOwnedOrNotFoundAsync(
                request.TrainingProgramId,
                cancellationToken);
            if (programResult.IsFailure)
            {
                return Result<AssignTrainingProgramToClientResult>.Failure(programResult.Error);
            }

            var program = programResult.Value;
            var hasRelationship = await _usersModule.HasTrainerClientRelationshipAsync(
                program.TrainerId,
                request.ClientId,
                cancellationToken);
            if (!hasRelationship)
            {
                return Result<AssignTrainingProgramToClientResult>.Failure(
                    Error.NotFound("client.not_found", "Client not found"));
            }

            var utcNow = _clock.UtcNow;
            var existingAssignment = await _assignmentRepository.GetActiveByClientAndProgramAsync(
                request.ClientId,
                program.Id,
                cancellationToken);

            if (existingAssignment is not null)
            {
                if (existingAssignment.IsAccessible(utcNow))
                {
                    return Result<AssignTrainingProgramToClientResult>.Failure(
                        Error.Conflict("assignment.already_exists", "Active assignment already exists"));
                }

                var expireResult = existingAssignment.Expire(utcNow);
                if (expireResult.IsFailure)
                {
                    return Result<AssignTrainingProgramToClientResult>.Failure(expireResult.Error);
                }
            }

            var assignmentResult = AssignedTrainingProgram.AssignManually(
                program.TrainerId,
                request.ClientId,
                program.Id,
                utcNow,
                request.ExpiresAtUtc);
            if (assignmentResult.IsFailure)
            {
                return Result<AssignTrainingProgramToClientResult>.Failure(assignmentResult.Error);
            }

            var assignment = assignmentResult.Value;

            if (existingAssignment is null)
            {
                await _assignmentRepository.AddAsync(assignment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _assignmentRepository.AddAsync(assignment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            return Result<AssignTrainingProgramToClientResult>.Success(
                new AssignTrainingProgramToClientResult(
                    assignment.Id,
                    assignment.TrainingProgramId,
                    assignment.ClientId,
                    assignment.Status.ToString(),
                    assignment.AccessSource.ToString(),
                    assignment.AssignedAtUtc,
                    assignment.ExpiresAtUtc));
        }
    }
}
