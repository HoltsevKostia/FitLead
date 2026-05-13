using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Domain.Trainings.TrainingProgramAssignments
{
    public sealed class AssignedTrainingProgram : AggregateRoot<Guid>
    {
        public Guid TrainerId { get; private set; }
        public Guid ClientId { get; private set; }
        public Guid TrainingProgramId { get; private set; }
        public AssignedProgramStatus Status { get; private set; }
        public ProgramAccessSource AccessSource { get; private set; }
        public DateTime AssignedAtUtc { get; private set; }
        public DateTime? RevokedAtUtc { get; private set; }
        public DateTime? ExpiresAtUtc { get; private set; }

        private AssignedTrainingProgram()
        {
        }

        private AssignedTrainingProgram(
            Guid id,
            Guid trainerId,
            Guid clientId,
            Guid trainingProgramId,
            ProgramAccessSource accessSource,
            DateTime assignedAtUtc,
            DateTime? expiresAtUtc)
        {
            Id = id;
            TrainerId = trainerId;
            ClientId = clientId;
            TrainingProgramId = trainingProgramId;
            Status = AssignedProgramStatus.Active;
            AccessSource = accessSource;
            AssignedAtUtc = assignedAtUtc;
            ExpiresAtUtc = expiresAtUtc;
        }

        public static Result<AssignedTrainingProgram> AssignManually(
            Guid trainerId,
            Guid clientId,
            Guid trainingProgramId,
            DateTime utcNow,
            DateTime? expiresAtUtc = null)
        {
            return Create(
                trainerId,
                clientId,
                trainingProgramId,
                ProgramAccessSource.Manual,
                utcNow,
                expiresAtUtc);
        }

        public Result Revoke(DateTime utcNow)
        {
            if (Status != AssignedProgramStatus.Active)
            {
                return Result.Failure(
                    Error.Conflict("training_program.assignment.revoke.invalid_status", "Only active assignment can be revoked"));
            }

            Status = AssignedProgramStatus.Revoked;
            RevokedAtUtc = utcNow;

            return Result.Success();
        }

        public Result Expire(DateTime utcNow)
        {
            if (Status != AssignedProgramStatus.Active)
            {
                return Result.Failure(
                    Error.Conflict("training_program.assignment.expire.invalid_status", "Only active assignment can be expired"));
            }

            if (!ExpiresAtUtc.HasValue || ExpiresAtUtc.Value > utcNow)
            {
                return Result.Failure(
                    Error.Conflict("training_program.assignment.expire.not_expired", "Assignment has not reached its expiration time"));
            }

            Status = AssignedProgramStatus.Expired;

            return Result.Success();
        }

        public bool IsAccessible(DateTime utcNow)
        {
            return Status == AssignedProgramStatus.Active &&
                   (!ExpiresAtUtc.HasValue || ExpiresAtUtc.Value > utcNow);
        }

        private static Result<AssignedTrainingProgram> Create(
            Guid trainerId,
            Guid clientId,
            Guid trainingProgramId,
            ProgramAccessSource accessSource,
            DateTime assignedAtUtc,
            DateTime? expiresAtUtc)
        {
            if (trainerId == Guid.Empty)
            {
                return Result<AssignedTrainingProgram>.Failure(
                    Error.Validation("training_program.assignment.create.trainer_id_required", "TrainerId is required"));
            }

            if (clientId == Guid.Empty)
            {
                return Result<AssignedTrainingProgram>.Failure(
                    Error.Validation("training_program.assignment.create.client_id_required", "ClientId is required"));
            }

            if (trainingProgramId == Guid.Empty)
            {
                return Result<AssignedTrainingProgram>.Failure(
                    Error.Validation("training_program.assignment.create.training_program_id_required", "TrainingProgramId is required"));
            }

            if (assignedAtUtc == default)
            {
                return Result<AssignedTrainingProgram>.Failure(
                    Error.Validation("training_program.assignment.create.assigned_at_required", "AssignedAtUtc is required"));
            }

            if (expiresAtUtc.HasValue && expiresAtUtc.Value <= assignedAtUtc)
            {
                return Result<AssignedTrainingProgram>.Failure(
                    Error.Validation("training_program.assignment.create.expires_at_invalid", "ExpiresAtUtc must be after AssignedAtUtc"));
            }

            return Result<AssignedTrainingProgram>.Success(
                new AssignedTrainingProgram(
                    Guid.NewGuid(),
                    trainerId,
                    clientId,
                    trainingProgramId,
                    accessSource,
                    assignedAtUtc,
                    expiresAtUtc));
        }
    }
}
