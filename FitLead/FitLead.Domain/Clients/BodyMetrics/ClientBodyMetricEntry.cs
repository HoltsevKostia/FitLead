using FitLead.Common.Domain;
using FitLead.Common.Results;
using DomainError = FitLead.Common.Errors.Error;

namespace FitLead.Domain.Clients.BodyMetrics
{
    public sealed class ClientBodyMetricEntry : AggregateRoot<Guid>
    {
        public const int MaxNoteLength = 1000;
        public const int MinWeightKg = 1;
        public const int MaxWeightKg = 500;
        public const int MinBodyFatPercent = 1;
        public const int MaxBodyFatPercent = 80;
        public const int MinMeasurementCm = 1;
        public const int MaxMeasurementCm = 300;

        public Guid ClientId { get; private set; }
        public DateOnly RecordedAt { get; private set; }
        public decimal? WeightKg { get; private set; }
        public decimal? BodyFatPercent { get; private set; }
        public decimal? ChestCm { get; private set; }
        public decimal? WaistCm { get; private set; }
        public decimal? HipsCm { get; private set; }
        public decimal? ArmCm { get; private set; }
        public decimal? ThighCm { get; private set; }
        public string? Note { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }

        private ClientBodyMetricEntry()
        {
        }

        private ClientBodyMetricEntry(
            Guid id,
            Guid clientId,
            DateOnly recordedAt,
            decimal? weightKg,
            decimal? bodyFatPercent,
            decimal? chestCm,
            decimal? waistCm,
            decimal? hipsCm,
            decimal? armCm,
            decimal? thighCm,
            string? note,
            DateTime createdAtUtc)
        {
            Id = id;
            ClientId = clientId;
            RecordedAt = recordedAt;
            WeightKg = weightKg;
            BodyFatPercent = bodyFatPercent;
            ChestCm = chestCm;
            WaistCm = waistCm;
            HipsCm = hipsCm;
            ArmCm = armCm;
            ThighCm = thighCm;
            Note = note;
            CreatedAtUtc = createdAtUtc;
        }

        public static Result<ClientBodyMetricEntry> Create(
            Guid clientId,
            DateOnly recordedAt,
            decimal? weightKg,
            decimal? bodyFatPercent,
            decimal? chestCm,
            decimal? waistCm,
            decimal? hipsCm,
            decimal? armCm,
            decimal? thighCm,
            string? note,
            DateTime createdAtUtc)
        {
            if (clientId == Guid.Empty)
            {
                return Result<ClientBodyMetricEntry>.Failure(
                    DomainError.Validation(
                        "body_metric_entry.create.client_id_required",
                        "ClientId is required"));
            }

            if (recordedAt == default)
            {
                return Result<ClientBodyMetricEntry>.Failure(
                    DomainError.Validation(
                        "body_metric_entry.create.recorded_at_required",
                        "RecordedAt is required"));
            }

            if (createdAtUtc == default)
            {
                return Result<ClientBodyMetricEntry>.Failure(
                    DomainError.Validation(
                        "body_metric_entry.create.created_at_required",
                        "CreatedAtUtc is required"));
            }

            var validationResult = ValidateFields(
                weightKg,
                bodyFatPercent,
                chestCm,
                waistCm,
                hipsCm,
                armCm,
                thighCm,
                note,
                operation: "create",
                out var normalizedNote);
            if (validationResult.IsFailure)
            {
                return Result<ClientBodyMetricEntry>.Failure(validationResult.Error);
            }

            return Result<ClientBodyMetricEntry>.Success(
                new ClientBodyMetricEntry(
                    Guid.NewGuid(),
                    clientId,
                    recordedAt,
                    weightKg,
                    bodyFatPercent,
                    chestCm,
                    waistCm,
                    hipsCm,
                    armCm,
                    thighCm,
                    normalizedNote,
                    createdAtUtc));
        }

        public Result Update(
            DateOnly recordedAt,
            decimal? weightKg,
            decimal? bodyFatPercent,
            decimal? chestCm,
            decimal? waistCm,
            decimal? hipsCm,
            decimal? armCm,
            decimal? thighCm,
            string? note,
            DateTime updatedAtUtc)
        {
            if (recordedAt == default)
            {
                return Result.Failure(
                    DomainError.Validation(
                        "body_metric_entry.update.recorded_at_required",
                        "RecordedAt is required"));
            }

            if (updatedAtUtc == default)
            {
                return Result.Failure(
                    DomainError.Validation(
                        "body_metric_entry.update.updated_at_required",
                        "UpdatedAtUtc is required"));
            }

            if (updatedAtUtc < CreatedAtUtc)
            {
                return Result.Failure(
                    DomainError.Validation(
                        "body_metric_entry.update.updated_at_before_created",
                        "UpdatedAtUtc cannot be earlier than CreatedAtUtc"));
            }

            var validationResult = ValidateFields(
                weightKg,
                bodyFatPercent,
                chestCm,
                waistCm,
                hipsCm,
                armCm,
                thighCm,
                note,
                operation: "update",
                out var normalizedNote);
            if (validationResult.IsFailure)
            {
                return validationResult;
            }

            RecordedAt = recordedAt;
            WeightKg = weightKg;
            BodyFatPercent = bodyFatPercent;
            ChestCm = chestCm;
            WaistCm = waistCm;
            HipsCm = hipsCm;
            ArmCm = armCm;
            ThighCm = thighCm;
            Note = normalizedNote;
            UpdatedAtUtc = updatedAtUtc;

            return Result.Success();
        }

        private static Result ValidateFields(
            decimal? weightKg,
            decimal? bodyFatPercent,
            decimal? chestCm,
            decimal? waistCm,
            decimal? hipsCm,
            decimal? armCm,
            decimal? thighCm,
            string? note,
            string operation,
            out string? normalizedNote)
        {
            normalizedNote = null;

            var noteResult = NormalizeNote(note, operation, out normalizedNote);
            if (noteResult.IsFailure)
            {
                return noteResult;
            }

            var weightResult = ValidateRange(
                weightKg,
                MinWeightKg,
                MaxWeightKg,
                "weight_kg",
                operation);
            if (weightResult.IsFailure)
            {
                return weightResult;
            }

            var bodyFatResult = ValidateRange(
                bodyFatPercent,
                MinBodyFatPercent,
                MaxBodyFatPercent,
                "body_fat_percent",
                operation);
            if (bodyFatResult.IsFailure)
            {
                return bodyFatResult;
            }

            var chestResult = ValidateRange(
                chestCm,
                MinMeasurementCm,
                MaxMeasurementCm,
                "chest_cm",
                operation);
            if (chestResult.IsFailure)
            {
                return chestResult;
            }

            var waistResult = ValidateRange(
                waistCm,
                MinMeasurementCm,
                MaxMeasurementCm,
                "waist_cm",
                operation);
            if (waistResult.IsFailure)
            {
                return waistResult;
            }

            var hipsResult = ValidateRange(
                hipsCm,
                MinMeasurementCm,
                MaxMeasurementCm,
                "hips_cm",
                operation);
            if (hipsResult.IsFailure)
            {
                return hipsResult;
            }

            var armResult = ValidateRange(
                armCm,
                MinMeasurementCm,
                MaxMeasurementCm,
                "arm_cm",
                operation);
            if (armResult.IsFailure)
            {
                return armResult;
            }

            var thighResult = ValidateRange(
                thighCm,
                MinMeasurementCm,
                MaxMeasurementCm,
                "thigh_cm",
                operation);
            if (thighResult.IsFailure)
            {
                return thighResult;
            }

            if (!weightKg.HasValue &&
                !bodyFatPercent.HasValue &&
                !chestCm.HasValue &&
                !waistCm.HasValue &&
                !hipsCm.HasValue &&
                !armCm.HasValue &&
                !thighCm.HasValue &&
                normalizedNote is null)
            {
                return Result.Failure(
                    DomainError.Validation(
                        $"body_metric_entry.{operation}.empty_entry",
                        "At least one metric value or note is required"));
            }

            return Result.Success();
        }

        private static Result ValidateRange(
            decimal? value,
            int min,
            int max,
            string fieldName,
            string operation)
        {
            if (!value.HasValue)
            {
                return Result.Success();
            }

            if (value.Value < min || value.Value > max)
            {
                return Result.Failure(
                    DomainError.Validation(
                        $"body_metric_entry.{operation}.{fieldName}_out_of_range",
                        $"{fieldName} must be between {min} and {max}"));
            }

            return Result.Success();
        }

        private static Result NormalizeNote(
            string? note,
            string operation,
            out string? normalizedNote)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                normalizedNote = null;
                return Result.Success();
            }

            var trimmedNote = note.Trim();
            if (trimmedNote.Length > MaxNoteLength)
            {
                normalizedNote = null;
                return Result.Failure(
                    DomainError.Validation(
                        $"body_metric_entry.{operation}.note_too_long",
                        $"Note cannot exceed {MaxNoteLength} characters"));
            }

            normalizedNote = trimmedNote;
            return Result.Success();
        }
    }
}
