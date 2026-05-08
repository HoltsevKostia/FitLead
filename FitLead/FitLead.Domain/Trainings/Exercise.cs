using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Domain.Trainings
{
    public sealed class Exercise : AggregateRoot<Guid>
    {
        public Guid? OwnerTrainerId { get; private set; }
        public ExerciseSource Source { get; private set; }
        public Guid? CopiedFromExerciseId { get; private set; }

        public string Name { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public MediaUrl? MediaUrl { get; private set; }
        public MuscleGroup? MuscleGroup { get; private set; }
        public Equipment? Equipment { get; private set; }

        private Exercise() { } // EF

        private Exercise(
            Guid id,
            Guid? ownerTrainerId,
            ExerciseSource source,
            string name,
            string description,
            MediaUrl? mediaUrl,
            MuscleGroup? muscleGroup,
            Equipment? equipment,
            Guid? copiedFromExerciseId)
        {
            Id = id;
            OwnerTrainerId = ownerTrainerId;
            Source = source;
            Name = name;
            Description = description;
            MediaUrl = mediaUrl;
            MuscleGroup = muscleGroup;
            Equipment = equipment;
            CopiedFromExerciseId = copiedFromExerciseId;
        }

        public static Result<Exercise> CreateTrainerExercise(
            Guid ownerTrainerId,
            string name,
            string description,
            string? mediaUrl = null,
            MuscleGroup? muscleGroup = null,
            Equipment? equipment = null)
        {
            return Create(
                ownerTrainerId,
                ExerciseSource.Trainer,
                name,
                description,
                mediaUrl,
                muscleGroup,
                equipment,
                copiedFromExerciseId: null);
        }

        public static Result<Exercise> CopyFromPlatformExercise(
            Guid ownerTrainerId,
            Exercise platformExercise)
        {
            if (platformExercise.Source != ExerciseSource.Platform)
            {
                return Result<Exercise>.Failure(
                    Error.Validation(
                        "exercise.copy.source_must_be_platform",
                        "Only platform exercises can be copied to trainer library"));
            }

            return Create(
                ownerTrainerId,
                ExerciseSource.Trainer,
                platformExercise.Name,
                platformExercise.Description,
                platformExercise.MediaUrl?.Value,
                platformExercise.MuscleGroup,
                platformExercise.Equipment,
                copiedFromExerciseId: platformExercise.Id);
        }

        public static Result<Exercise> CreatePlatformExercise(
            string name,
            string description,
            string? mediaUrl = null,
            MuscleGroup? muscleGroup = null,
            Equipment? equipment = null)
        {
            return Create(
                ownerTrainerId: null,
                ExerciseSource.Platform,
                name,
                description,
                mediaUrl,
                muscleGroup,
                equipment,
                copiedFromExerciseId: null);
        }

        private static Result<Exercise> Create(
            Guid? ownerTrainerId,
            ExerciseSource source,
            string name,
            string description,
            string? mediaUrl,
            MuscleGroup? muscleGroup,
            Equipment? equipment,
            Guid? copiedFromExerciseId)
        {
            var ownershipResult = ValidateOwnership(source, ownerTrainerId);
            if (ownershipResult.IsFailure)
                return Result<Exercise>.Failure(ownershipResult.Error);

            if (copiedFromExerciseId == Guid.Empty)
                return Result<Exercise>.Failure(
                    Error.Validation("exercise.create.copied_from_exercise_id_invalid", "CopiedFromExerciseId must be null or a valid id"));

            if (source == ExerciseSource.Platform && copiedFromExerciseId.HasValue)
                return Result<Exercise>.Failure(
                    Error.Validation("exercise.create.platform_copied_from_not_allowed", "Platform exercise cannot be copied from another exercise"));

            if (string.IsNullOrWhiteSpace(name))
                return Result<Exercise>.Failure(
                    Error.Validation("exercise.create.name_required", "Exercise name is required"));

            var mediaUrlResult = TryCreateMediaUrl(mediaUrl, out var parsedMediaUrl);
            if (mediaUrlResult.IsFailure)
                return Result<Exercise>.Failure(mediaUrlResult.Error);

            return Result<Exercise>.Success(
                new Exercise(
                    Guid.NewGuid(),
                    ownerTrainerId,
                    source,
                    name.Trim(),
                    description?.Trim() ?? string.Empty,
                    parsedMediaUrl,
                    muscleGroup,
                    equipment,
                    copiedFromExerciseId));
        }

        private Result Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(
                    Error.Validation("exercise.update.name.required", "Exercise name is required"));

            Name = name.Trim();
            return Result.Success();
        }

        private void UpdateDescription(string description)
        {
            Description = description?.Trim() ?? string.Empty;
        }

        private Result UpdateMediaUrl(string? mediaUrl)
        {
            var mediaUrlResult = TryCreateMediaUrl(mediaUrl, out var parsedMediaUrl);
            if (mediaUrlResult.IsFailure)
                return Result.Failure(mediaUrlResult.Error);

            MediaUrl = parsedMediaUrl;

            return Result.Success();
        }

        private static Result TryCreateMediaUrl(string? mediaUrl, out MediaUrl? parsedMediaUrl)
        {
            parsedMediaUrl = null;

            if (string.IsNullOrWhiteSpace(mediaUrl))
                return Result.Success();

            var result = MediaUrl.Create(mediaUrl);
            if (result.IsFailure)
                return Result.Failure(result.Error);

            parsedMediaUrl = result.Value;
            return Result.Success();
        }

        private static Result ValidateOwnership(ExerciseSource source, Guid? ownerTrainerId)
        {
            return source switch
            {
                ExerciseSource.Platform when ownerTrainerId.HasValue =>
                    Result.Failure(Error.Validation(
                        "exercise.ownership.platform_owner_not_allowed",
                        "Platform exercise cannot have an owner trainer")),

                ExerciseSource.Trainer when !ownerTrainerId.HasValue || ownerTrainerId.Value == Guid.Empty =>
                    Result.Failure(Error.Validation(
                        "exercise.ownership.trainer_owner_required",
                        "Trainer exercise must have an owner trainer")),

                ExerciseSource.Platform or ExerciseSource.Trainer => Result.Success(),

                _ => Result.Failure(Error.Validation(
                    "exercise.ownership.source_invalid",
                    "Exercise source is invalid"))
            };
        }

        public Result UpdateByTrainer(
            Guid trainerId,
            string name,
            string description,
            string? mediaUrl,
            MuscleGroup? muscleGroup,
            Equipment? equipment)
        {
            if (Source != ExerciseSource.Trainer || OwnerTrainerId != trainerId)
            {
                return Result.Failure(
                    Error.Forbidden(
                        "exercise.update.not_allowed",
                        "Only own trainer exercises can be updated"));
            }

            return Update(name, description, mediaUrl, muscleGroup, equipment);
        }

        private Result Update(
            string name,
            string description,
            string? mediaUrl,
            MuscleGroup? muscleGroup,
            Equipment? equipment)
        {
            var renameResult = Rename(name);
            if (renameResult.IsFailure)
                return renameResult;

            UpdateDescription(description);

            var mediaResult = UpdateMediaUrl(mediaUrl);
            if (mediaResult.IsFailure)
                return mediaResult;

            MuscleGroup = muscleGroup;
            Equipment = equipment;

            return Result.Success();
        }
    }
}
