using FitLead.Application.Abstractions.Persistence;
using FitLead.Application.Common;
using FitLead.Application.Common.Time;
using FitLead.Application.Trainings.WorkoutLogs.Access;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Domain.Trainings.WorkoutLogs;
using MediatR;

namespace FitLead.Application.Trainings.WorkoutLogs.Commands
{
    public sealed class UpsertWorkoutLogHandler
        : IRequestHandler<UpsertWorkoutLogCommand, Result<WorkoutLogDto>>
    {
        private readonly IWorkoutLogAccessLoader _accessLoader;
        private readonly IWorkoutLogRepository _workoutLogRepository;
        private readonly IClock _clock;
        private readonly IUnitOfWork _unitOfWork;

        public UpsertWorkoutLogHandler(
            IWorkoutLogAccessLoader accessLoader,
            IWorkoutLogRepository workoutLogRepository,
            IClock clock,
            IUnitOfWork unitOfWork)
        {
            _accessLoader = accessLoader;
            _workoutLogRepository = workoutLogRepository;
            _clock = clock;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<WorkoutLogDto>> Handle(
            UpsertWorkoutLogCommand request,
            CancellationToken cancellationToken)
        {
            var statusResult = EnumParser.ParseDefined<WorkoutLogStatus>(
                request.Status,
                "workout_log.status_required",
                "Status is required",
                "workout_log.status_invalid",
                "Status is invalid");
            if (statusResult.IsFailure)
            {
                return Result<WorkoutLogDto>.Failure(statusResult.Error);
            }

            var utcNow = _clock.UtcNow;
            var requestFieldsResult = ValidateRequestFields(
                statusResult.Value,
                request.PerformedAtUtc,
                request.DifficultyRating);
            if (requestFieldsResult.IsFailure)
            {
                return Result<WorkoutLogDto>.Failure(requestFieldsResult.Error);
            }

            var accessResult = await _accessLoader.GetForCurrentClientOrNotFoundAsync(
                request.AssignmentId,
                request.TrainingProgramWorkoutId,
                cancellationToken);
            if (accessResult.IsFailure)
            {
                return Result<WorkoutLogDto>.Failure(accessResult.Error);
            }

            var access = accessResult.Value;
            var existingLog = await _workoutLogRepository.GetByAssignmentWorkoutAsync(
                access.AssignedTrainingProgramId,
                access.TrainingProgramWorkoutId,
                cancellationToken);

            if (existingLog is null)
            {
                var createResult = CreateWorkoutLog(
                access,
                statusResult.Value,
                GetPerformedAtUtc(statusResult.Value, request.PerformedAtUtc, utcNow),
                request.ClientNote,
                request.DifficultyRating,
                utcNow);
                if (createResult.IsFailure)
                {
                    return Result<WorkoutLogDto>.Failure(createResult.Error);
                }

                await _workoutLogRepository.AddAsync(createResult.Value, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<WorkoutLogDto>.Success(ToDto(createResult.Value));
            }

            var updateResult = existingLog.Update(
                statusResult.Value,
                GetPerformedAtUtc(statusResult.Value, request.PerformedAtUtc, utcNow),
                request.ClientNote,
                request.DifficultyRating,
                utcNow);
            if (updateResult.IsFailure)
            {
                return Result<WorkoutLogDto>.Failure(updateResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<WorkoutLogDto>.Success(ToDto(existingLog));
        }

        private static DateTime? GetPerformedAtUtc(
            WorkoutLogStatus status,
            DateTime? requestedPerformedAtUtc,
            DateTime utcNow)
        {
            return status == WorkoutLogStatus.Completed
                ? requestedPerformedAtUtc ?? utcNow
                : null;
        }

        private static Result ValidateRequestFields(
            WorkoutLogStatus status,
            DateTime? performedAtUtc,
            int? difficultyRating)
        {
            if (status == WorkoutLogStatus.Skipped && performedAtUtc.HasValue)
            {
                return Result.Failure(
                    Error.Validation(
                        "workout_log.skipped_performed_at_not_allowed",
                        "PerformedAtUtc is not allowed for skipped workout logs"));
            }

            if (status == WorkoutLogStatus.Skipped && difficultyRating.HasValue)
            {
                return Result.Failure(
                    Error.Validation(
                        "workout_log.skipped_difficulty_rating_not_allowed",
                        "DifficultyRating is not allowed for skipped workout logs"));
            }

            return Result.Success();
        }

        private static Result<WorkoutLog> CreateWorkoutLog(
            WorkoutLogAccessContext access,
            WorkoutLogStatus status,
            DateTime? performedAtUtc,
            string? clientNote,
            int? difficultyRating,
            DateTime createdAtUtc)
        {
            return status switch
            {
                WorkoutLogStatus.Completed => WorkoutLog.CreateCompleted(
                    access.AssignedTrainingProgramId,
                    access.TrainingProgramWorkoutId,
                    access.ClientId,
                    access.TrainerId,
                    performedAtUtc ?? default,
                    clientNote,
                    difficultyRating,
                    createdAtUtc),
                WorkoutLogStatus.Skipped => WorkoutLog.CreateSkipped(
                    access.AssignedTrainingProgramId,
                    access.TrainingProgramWorkoutId,
                    access.ClientId,
                    access.TrainerId,
                    clientNote,
                    createdAtUtc),
                _ => Result<WorkoutLog>.Failure(
                    Error.Validation("workout_log.status_invalid", "Status is invalid"))
            };
        }

        private static WorkoutLogDto ToDto(WorkoutLog workoutLog)
        {
            return new WorkoutLogDto(
                workoutLog.Id,
                workoutLog.Status.ToString(),
                workoutLog.PerformedAtUtc,
                workoutLog.ClientNote,
                workoutLog.DifficultyRating,
                workoutLog.CreatedAtUtc,
                workoutLog.UpdatedAtUtc);
        }
    }
}
