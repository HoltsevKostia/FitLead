using FitLead.Common.Domain;
using FitLead.Common.Results;
using DomainError = FitLead.Common.Errors.Error;

namespace FitLead.Domain.Outbox
{
    public sealed class OutboxMessage : Entity<Guid>
    {
        public const int MaxTypeLength = 200;
        public const int MaxErrorLength = 4000;

        public string Type { get; private set; } = string.Empty;
        public string Payload { get; private set; } = "{}";
        public DateTime OccurredAtUtc { get; private set; }
        public OutboxMessageStatus Status { get; private set; }
        public int RetryCount { get; private set; }
        public DateTime? NextRetryAtUtc { get; private set; }
        public DateTime? ProcessedAtUtc { get; private set; }
        public string? Error { get; private set; }

        private OutboxMessage()
        {
        }

        private OutboxMessage(
            Guid id,
            string type,
            string payload,
            DateTime occurredAtUtc)
        {
            Id = id;
            Type = type;
            Payload = payload;
            OccurredAtUtc = occurredAtUtc;
            Status = OutboxMessageStatus.Pending;
        }

        public static Result<OutboxMessage> Create(
            string type,
            string payload,
            DateTime occurredAtUtc)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return Result<OutboxMessage>.Failure(
                    DomainError.Validation("outbox_message.create.type_required", "Type is required"));
            }

            var trimmedType = type.Trim();
            if (trimmedType.Length > MaxTypeLength)
            {
                return Result<OutboxMessage>.Failure(
                    DomainError.Validation("outbox_message.create.type_too_long", $"Type cannot exceed {MaxTypeLength} characters"));
            }

            if (string.IsNullOrWhiteSpace(payload))
            {
                return Result<OutboxMessage>.Failure(
                    DomainError.Validation("outbox_message.create.payload_required", "Payload is required"));
            }

            if (occurredAtUtc == default)
            {
                return Result<OutboxMessage>.Failure(
                    DomainError.Validation("outbox_message.create.occurred_at_required", "OccurredAtUtc is required"));
            }

            return Result<OutboxMessage>.Success(
                new OutboxMessage(
                    Guid.NewGuid(),
                    trimmedType,
                    payload,
                    occurredAtUtc));
        }

        public Result MarkProcessed(DateTime processedAtUtc)
        {
            if (processedAtUtc == default)
            {
                return Result.Failure(
                    DomainError.Validation("outbox_message.mark_processed.processed_at_required", "ProcessedAtUtc is required"));
            }

            if (Status == OutboxMessageStatus.Processed)
            {
                return Result.Failure(
                    DomainError.Conflict("outbox_message.already_processed", "Outbox message is already processed"));
            }

            if (Status == OutboxMessageStatus.Failed)
            {
                return Result.Failure(
                    DomainError.Conflict("outbox_message.already_failed", "Failed outbox message cannot be marked as processed"));
            }

            Status = OutboxMessageStatus.Processed;
            ProcessedAtUtc = processedAtUtc;
            NextRetryAtUtc = null;
            Error = null;

            return Result.Success();
        }

        public Result MarkFailedOrRetry(
            DateTime utcNow,
            int maxAttempts,
            TimeSpan retryDelay,
            string error)
        {
            if (utcNow == default)
            {
                return Result.Failure(
                    DomainError.Validation("outbox_message.mark_failed.utc_now_required", "UtcNow is required"));
            }

            if (maxAttempts <= 0)
            {
                return Result.Failure(
                    DomainError.Validation("outbox_message.mark_failed.max_attempts_invalid", "MaxAttempts must be greater than zero"));
            }

            if (retryDelay < TimeSpan.Zero)
            {
                return Result.Failure(
                    DomainError.Validation("outbox_message.mark_failed.retry_delay_invalid", "RetryDelay cannot be negative"));
            }

            if (Status == OutboxMessageStatus.Processed)
            {
                return Result.Failure(
                    DomainError.Conflict("outbox_message.already_processed", "Processed outbox message cannot be retried"));
            }

            RetryCount++;
            Error = NormalizeError(error);

            if (RetryCount >= maxAttempts)
            {
                Status = OutboxMessageStatus.Failed;
                NextRetryAtUtc = null;
            }
            else
            {
                Status = OutboxMessageStatus.Pending;
                NextRetryAtUtc = utcNow.Add(retryDelay);
            }

            return Result.Success();
        }

        private static string? NormalizeError(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return null;
            }

            var trimmedError = error.Trim();

            return trimmedError.Length <= MaxErrorLength
                ? trimmedError
                : trimmedError[..MaxErrorLength];
        }
    }
}
