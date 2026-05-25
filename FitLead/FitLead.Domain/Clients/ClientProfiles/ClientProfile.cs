using FitLead.Common.Domain;
using FitLead.Common.Results;
using DomainError = FitLead.Common.Errors.Error;

namespace FitLead.Domain.Clients.ClientProfiles
{
    public sealed class ClientProfile : AggregateRoot<Guid>
    {
        public const int MaxGoalLength = 500;
        public const int MaxLongTextLength = 1000;
        public const int MinHeightCm = 50;
        public const int MaxHeightCm = 300;

        public Guid ClientId { get; private set; }
        public string? Goal { get; private set; }
        public ClientExperienceLevel? ExperienceLevel { get; private set; }
        public int? HeightCm { get; private set; }
        public string? Limitations { get; private set; }
        public string? TrainingPreferences { get; private set; }
        public string? AdditionalInfo { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }

        private ClientProfile()
        {
        }

        private ClientProfile(
            Guid id,
            Guid clientId,
            string? goal,
            ClientExperienceLevel? experienceLevel,
            int? heightCm,
            string? limitations,
            string? trainingPreferences,
            string? additionalInfo,
            DateTime createdAtUtc)
        {
            Id = id;
            ClientId = clientId;
            Goal = goal;
            ExperienceLevel = experienceLevel;
            HeightCm = heightCm;
            Limitations = limitations;
            TrainingPreferences = trainingPreferences;
            AdditionalInfo = additionalInfo;
            CreatedAtUtc = createdAtUtc;
        }

        public static Result<ClientProfile> Create(
            Guid clientId,
            string? goal,
            ClientExperienceLevel? experienceLevel,
            int? heightCm,
            string? limitations,
            string? trainingPreferences,
            string? additionalInfo,
            DateTime createdAtUtc)
        {
            if (clientId == Guid.Empty)
            {
                return Result<ClientProfile>.Failure(
                    DomainError.Validation("client_profile.create.client_id_required", "ClientId is required"));
            }

            if (createdAtUtc == default)
            {
                return Result<ClientProfile>.Failure(
                    DomainError.Validation("client_profile.create.created_at_required", "CreatedAtUtc is required"));
            }

            var validationResult = ValidateFields(
                goal,
                experienceLevel,
                heightCm,
                limitations,
                trainingPreferences,
                additionalInfo,
                operation: "create",
                out var normalizedGoal,
                out var normalizedLimitations,
                out var normalizedTrainingPreferences,
                out var normalizedAdditionalInfo);
            if (validationResult.IsFailure)
            {
                return Result<ClientProfile>.Failure(validationResult.Error);
            }

            return Result<ClientProfile>.Success(
                new ClientProfile(
                    Guid.NewGuid(),
                    clientId,
                    normalizedGoal,
                    experienceLevel,
                    heightCm,
                    normalizedLimitations,
                    normalizedTrainingPreferences,
                    normalizedAdditionalInfo,
                    createdAtUtc));
        }

        public Result Update(
            string? goal,
            ClientExperienceLevel? experienceLevel,
            int? heightCm,
            string? limitations,
            string? trainingPreferences,
            string? additionalInfo,
            DateTime updatedAtUtc)
        {
            if (updatedAtUtc == default)
            {
                return Result.Failure(
                    DomainError.Validation("client_profile.update.updated_at_required", "UpdatedAtUtc is required"));
            }

            if (updatedAtUtc < CreatedAtUtc)
            {
                return Result.Failure(
                    DomainError.Validation("client_profile.update.updated_at_before_created", "UpdatedAtUtc cannot be earlier than CreatedAtUtc"));
            }

            var validationResult = ValidateFields(
                goal,
                experienceLevel,
                heightCm,
                limitations,
                trainingPreferences,
                additionalInfo,
                operation: "update",
                out var normalizedGoal,
                out var normalizedLimitations,
                out var normalizedTrainingPreferences,
                out var normalizedAdditionalInfo);
            if (validationResult.IsFailure)
            {
                return validationResult;
            }

            Goal = normalizedGoal;
            ExperienceLevel = experienceLevel;
            HeightCm = heightCm;
            Limitations = normalizedLimitations;
            TrainingPreferences = normalizedTrainingPreferences;
            AdditionalInfo = normalizedAdditionalInfo;
            UpdatedAtUtc = updatedAtUtc;

            return Result.Success();
        }

        private static Result ValidateFields(
            string? goal,
            ClientExperienceLevel? experienceLevel,
            int? heightCm,
            string? limitations,
            string? trainingPreferences,
            string? additionalInfo,
            string operation,
            out string? normalizedGoal,
            out string? normalizedLimitations,
            out string? normalizedTrainingPreferences,
            out string? normalizedAdditionalInfo)
        {
            normalizedGoal = null;
            normalizedLimitations = null;
            normalizedTrainingPreferences = null;
            normalizedAdditionalInfo = null;

            if (experienceLevel.HasValue && !Enum.IsDefined(experienceLevel.Value))
            {
                return Result.Failure(
                    DomainError.Validation($"client_profile.{operation}.experience_level_invalid", "ExperienceLevel is invalid"));
            }

            if (heightCm.HasValue &&
                (heightCm.Value < MinHeightCm || heightCm.Value > MaxHeightCm))
            {
                return Result.Failure(
                    DomainError.Validation(
                        $"client_profile.{operation}.height_out_of_range",
                        $"HeightCm must be between {MinHeightCm} and {MaxHeightCm}"));
            }

            var goalResult = NormalizeText(goal, MaxGoalLength, "goal", operation, out normalizedGoal);
            if (goalResult.IsFailure)
            {
                return goalResult;
            }

            var limitationsResult = NormalizeText(limitations, MaxLongTextLength, "limitations", operation, out normalizedLimitations);
            if (limitationsResult.IsFailure)
            {
                return limitationsResult;
            }

            var trainingPreferencesResult = NormalizeText(
                trainingPreferences,
                MaxLongTextLength,
                "training_preferences",
                operation,
                out normalizedTrainingPreferences);
            if (trainingPreferencesResult.IsFailure)
            {
                return trainingPreferencesResult;
            }

            return NormalizeText(
                additionalInfo,
                MaxLongTextLength,
                "additional_info",
                operation,
                out normalizedAdditionalInfo);
        }

        private static Result NormalizeText(
            string? value,
            int maxLength,
            string fieldName,
            string operation,
            out string? normalizedValue)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                normalizedValue = null;
                return Result.Success();
            }

            var trimmedValue = value.Trim();
            if (trimmedValue.Length > maxLength)
            {
                normalizedValue = null;
                return Result.Failure(
                    DomainError.Validation(
                        $"client_profile.{operation}.{fieldName}_too_long",
                        $"{fieldName} cannot exceed {maxLength} characters"));
            }

            normalizedValue = trimmedValue;
            return Result.Success();
        }
    }
}
