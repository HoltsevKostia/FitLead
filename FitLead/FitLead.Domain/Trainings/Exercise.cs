using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Domain.Trainings
{
    public sealed class Exercise : AggregateRoot<Guid>
    {
        public Guid TrainerId { get; private set; }

        public string Name { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public string? MediaUrl { get; private set; }

        private Exercise() { } // EF

        private Exercise(
            Guid id,
            Guid trainerId,
            string name,
            string description,
            string? mediaUrl)
        {
            Id = id;
            TrainerId = trainerId;
            Name = name;
            Description = description;
            MediaUrl = mediaUrl;
        }

        public static Result<Exercise> Create(
            Guid trainerId,
            string name,
            string description,
            string? mediaUrl = null)
        {
            if (trainerId == Guid.Empty)
                return Result<Exercise>.Failure(
                    Error.Validation("exercise.create.trainer_id_required", "TrainerId is required"));

            if (string.IsNullOrWhiteSpace(name))
                return Result<Exercise>.Failure(
                    Error.Validation("exercise.create.name_required", "Exercise name is required"));

            return Result<Exercise>.Success(
                new Exercise(
                    Guid.NewGuid(),
                    trainerId,
                    name.Trim(),
                    description?.Trim() ?? string.Empty,
                    mediaUrl));
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
            if (!string.IsNullOrWhiteSpace(mediaUrl))
            {
                var trimmed = mediaUrl.Trim();

                if (trimmed.Length > 500)
                    return Result.Failure(
                        Error.Validation("exercise.update.media_url.too_long", "MediaUrl is too long"));

                MediaUrl = trimmed;
            }
            else
            {
                MediaUrl = null;
            }

            return Result.Success();
        }

        public Result Update(string name, string description, string? mediaUrl)
        {
            var renameResult = Rename(name);
            if (renameResult.IsFailure)
                return renameResult;

            UpdateDescription(description);

            var mediaResult = UpdateMediaUrl(mediaUrl);
            if (mediaResult.IsFailure)
                return mediaResult;

            return Result.Success();
        }
    }
}
