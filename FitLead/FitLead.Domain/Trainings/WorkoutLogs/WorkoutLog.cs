using FitLead.Common.Domain;
using FitLead.Common.Results;
using DomainError = FitLead.Common.Errors.Error;

namespace FitLead.Domain.Trainings.WorkoutLogs
{
    public sealed class WorkoutLog : AggregateRoot<Guid>
    {
        public const int MaxClientNoteLength = 1000;
        public const int MinDifficultyRating = 1;
        public const int MaxDifficultyRating = 10;

        public Guid AssignedTrainingProgramId { get; private set; }
        public Guid TrainingProgramWorkoutId { get; private set; }
        public Guid ClientId { get; private set; }
        public Guid TrainerId { get; private set; }
        public WorkoutLogStatus Status { get; private set; }
        public DateTime? PerformedAtUtc { get; private set; }
        public string? ClientNote { get; private set; }
        public int? DifficultyRating { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }

        private WorkoutLog()
        {
        }

        private WorkoutLog(
            Guid id,
            Guid assignedTrainingProgramId,
            Guid trainingProgramWorkoutId,
            Guid clientId,
            Guid trainerId,
            WorkoutLogStatus status,
            DateTime? performedAtUtc,
            string? clientNote,
            int? difficultyRating,
            DateTime createdAtUtc)
        {
            Id = id;
            AssignedTrainingProgramId = assignedTrainingProgramId;
            TrainingProgramWorkoutId = trainingProgramWorkoutId;
            ClientId = clientId;
            TrainerId = trainerId;
            Status = status;
            PerformedAtUtc = performedAtUtc;
            ClientNote = clientNote;
            DifficultyRating = difficultyRating;
            CreatedAtUtc = createdAtUtc;
        }

        public static Result<WorkoutLog> CreateCompleted(
            Guid assignedTrainingProgramId,
            Guid trainingProgramWorkoutId,
            Guid clientId,
            Guid trainerId,
            DateTime performedAtUtc,
            string? clientNote,
            int? difficultyRating,
            DateTime createdAtUtc)
        {
            return Create(
                assignedTrainingProgramId,
                trainingProgramWorkoutId,
                clientId,
                trainerId,
                WorkoutLogStatus.Completed,
                performedAtUtc,
                clientNote,
                difficultyRating,
                createdAtUtc);
        }

        public static Result<WorkoutLog> CreateSkipped(
            Guid assignedTrainingProgramId,
            Guid trainingProgramWorkoutId,
            Guid clientId,
            Guid trainerId,
            string? clientNote,
            DateTime createdAtUtc)
        {
            return Create(
                assignedTrainingProgramId,
                trainingProgramWorkoutId,
                clientId,
                trainerId,
                WorkoutLogStatus.Skipped,
                performedAtUtc: null,
                clientNote,
                difficultyRating: null,
                createdAtUtc);
        }

        public Result Update(
            WorkoutLogStatus status,
            DateTime? performedAtUtc,
            string? clientNote,
            int? difficultyRating,
            DateTime updatedAtUtc)
        {
            if (updatedAtUtc == default)
            {
                return Result.Failure(
                    DomainError.Validation("workout_log.update.updated_at_required", "UpdatedAtUtc is required"));
            }

            if (updatedAtUtc < CreatedAtUtc)
            {
                return Result.Failure(
                    DomainError.Validation("workout_log.update.updated_at_before_created", "UpdatedAtUtc cannot be earlier than CreatedAtUtc"));
            }

            var validationResult = ValidateStatusFields(
                status,
                performedAtUtc,
                difficultyRating,
                operation: "update");
            if (validationResult.IsFailure)
            {
                return validationResult;
            }

            var noteResult = NormalizeClientNote(clientNote, operation: "update", out var normalizedClientNote);
            if (noteResult.IsFailure)
            {
                return noteResult;
            }

            Status = status;
            PerformedAtUtc = status == WorkoutLogStatus.Completed ? performedAtUtc : null;
            DifficultyRating = status == WorkoutLogStatus.Completed ? difficultyRating : null;
            ClientNote = normalizedClientNote;
            UpdatedAtUtc = updatedAtUtc;

            return Result.Success();
        }

        private static Result<WorkoutLog> Create(
            Guid assignedTrainingProgramId,
            Guid trainingProgramWorkoutId,
            Guid clientId,
            Guid trainerId,
            WorkoutLogStatus status,
            DateTime? performedAtUtc,
            string? clientNote,
            int? difficultyRating,
            DateTime createdAtUtc)
        {
            if (assignedTrainingProgramId == Guid.Empty)
            {
                return Result<WorkoutLog>.Failure(
                    DomainError.Validation("workout_log.create.assignment_id_required", "AssignedTrainingProgramId is required"));
            }

            if (trainingProgramWorkoutId == Guid.Empty)
            {
                return Result<WorkoutLog>.Failure(
                    DomainError.Validation("workout_log.create.program_workout_id_required", "TrainingProgramWorkoutId is required"));
            }

            if (clientId == Guid.Empty)
            {
                return Result<WorkoutLog>.Failure(
                    DomainError.Validation("workout_log.create.client_id_required", "ClientId is required"));
            }

            if (trainerId == Guid.Empty)
            {
                return Result<WorkoutLog>.Failure(
                    DomainError.Validation("workout_log.create.trainer_id_required", "TrainerId is required"));
            }

            if (createdAtUtc == default)
            {
                return Result<WorkoutLog>.Failure(
                    DomainError.Validation("workout_log.create.created_at_required", "CreatedAtUtc is required"));
            }

            var validationResult = ValidateStatusFields(
                status,
                performedAtUtc,
                difficultyRating,
                operation: "create");
            if (validationResult.IsFailure)
            {
                return Result<WorkoutLog>.Failure(validationResult.Error);
            }

            var noteResult = NormalizeClientNote(clientNote, operation: "create", out var normalizedClientNote);
            if (noteResult.IsFailure)
            {
                return Result<WorkoutLog>.Failure(noteResult.Error);
            }

            return Result<WorkoutLog>.Success(
                new WorkoutLog(
                    Guid.NewGuid(),
                    assignedTrainingProgramId,
                    trainingProgramWorkoutId,
                    clientId,
                    trainerId,
                    status,
                    status == WorkoutLogStatus.Completed ? performedAtUtc : null,
                    normalizedClientNote,
                    status == WorkoutLogStatus.Completed ? difficultyRating : null,
                    createdAtUtc));
        }

        private static Result ValidateStatusFields(
            WorkoutLogStatus status,
            DateTime? performedAtUtc,
            int? difficultyRating,
            string operation)
        {
            if (!Enum.IsDefined(status))
            {
                return Result.Failure(
                    DomainError.Validation($"workout_log.{operation}.status_invalid", "Workout log status is invalid"));
            }

            if (status == WorkoutLogStatus.Completed)
            {
                if (!performedAtUtc.HasValue || performedAtUtc.Value == default)
                {
                    return Result.Failure(
                        DomainError.Validation($"workout_log.{operation}.performed_at_required", "PerformedAtUtc is required for completed workout logs"));
                }

                if (difficultyRating.HasValue &&
                    (difficultyRating.Value < MinDifficultyRating ||
                     difficultyRating.Value > MaxDifficultyRating))
                {
                    return Result.Failure(
                        DomainError.Validation(
                            $"workout_log.{operation}.difficulty_rating_out_of_range",
                            $"DifficultyRating must be between {MinDifficultyRating} and {MaxDifficultyRating}"));
                }

                return Result.Success();
            }

            if (performedAtUtc.HasValue)
            {
                return Result.Failure(
                    DomainError.Validation($"workout_log.{operation}.skipped_performed_at_not_allowed", "PerformedAtUtc is not allowed for skipped workout logs"));
            }

            if (difficultyRating.HasValue)
            {
                return Result.Failure(
                    DomainError.Validation($"workout_log.{operation}.skipped_difficulty_rating_not_allowed", "DifficultyRating is not allowed for skipped workout logs"));
            }

            return Result.Success();
        }

        private static Result NormalizeClientNote(string? value, string operation, out string? normalizedValue)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                normalizedValue = null;
                return Result.Success();
            }

            var trimmedValue = value.Trim();
            if (trimmedValue.Length > MaxClientNoteLength)
            {
                normalizedValue = null;
                return Result.Failure(
                    DomainError.Validation(
                        $"workout_log.{operation}.client_note_too_long",
                        $"ClientNote cannot exceed {MaxClientNoteLength} characters"));
            }

            normalizedValue = trimmedValue;
            return Result.Success();
        }
    }
}
