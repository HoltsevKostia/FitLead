using FitLead.Common.Domain;
using FitLead.Common.Results;
using DomainError = FitLead.Common.Errors.Error;

namespace FitLead.Domain.Notifications
{
    public sealed class Notification : AggregateRoot<Guid>
    {
        public const int MaxTitleLength = 200;
        public const int MaxBodyLength = 1000;
        public const int MaxLinkUrlLength = 500;

        public Guid RecipientUserId { get; private set; }
        public NotificationType Type { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string? Body { get; private set; }
        public string LinkUrl { get; private set; } = string.Empty;
        public bool IsRead { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? ReadAtUtc { get; private set; }
        public Guid SourceEventId { get; private set; }

        private Notification()
        {
        }

        private Notification(
            Guid id,
            Guid recipientUserId,
            NotificationType type,
            string title,
            string? body,
            string linkUrl,
            DateTime createdAtUtc,
            Guid sourceEventId)
        {
            Id = id;
            RecipientUserId = recipientUserId;
            Type = type;
            Title = title;
            Body = body;
            LinkUrl = linkUrl;
            CreatedAtUtc = createdAtUtc;
            SourceEventId = sourceEventId;
        }

        public static Result<Notification> Create(
            Guid recipientUserId,
            NotificationType type,
            string title,
            string? body,
            string linkUrl,
            DateTime createdAtUtc,
            Guid sourceEventId)
        {
            if (recipientUserId == Guid.Empty)
            {
                return Result<Notification>.Failure(
                    DomainError.Validation("notification.create.recipient_user_id_required", "RecipientUserId is required"));
            }

            if (!Enum.IsDefined(type))
            {
                return Result<Notification>.Failure(
                    DomainError.Validation("notification.create.type_invalid", "Notification type is invalid"));
            }

            var titleResult = NormalizeRequiredText(
                title,
                MaxTitleLength,
                "notification.create.title_required",
                "Title is required",
                "notification.create.title_too_long",
                $"Title cannot exceed {MaxTitleLength} characters");
            if (titleResult.IsFailure)
            {
                return Result<Notification>.Failure(titleResult.Error);
            }

            var bodyResult = NormalizeOptionalText(
                body,
                MaxBodyLength,
                "notification.create.body_too_long",
                $"Body cannot exceed {MaxBodyLength} characters");
            if (bodyResult.IsFailure)
            {
                return Result<Notification>.Failure(bodyResult.Error);
            }

            var linkUrlResult = NormalizeLinkUrl(linkUrl);
            if (linkUrlResult.IsFailure)
            {
                return Result<Notification>.Failure(linkUrlResult.Error);
            }

            if (createdAtUtc == default)
            {
                return Result<Notification>.Failure(
                    DomainError.Validation("notification.create.created_at_required", "CreatedAtUtc is required"));
            }

            if (sourceEventId == Guid.Empty)
            {
                return Result<Notification>.Failure(
                    DomainError.Validation("notification.create.source_event_id_required", "SourceEventId is required"));
            }

            return Result<Notification>.Success(
                new Notification(
                    Guid.NewGuid(),
                    recipientUserId,
                    type,
                    titleResult.Value,
                    bodyResult.Value,
                    linkUrlResult.Value,
                    createdAtUtc,
                    sourceEventId));
        }

        public Result MarkRead(DateTime readAtUtc)
        {
            if (readAtUtc == default)
            {
                return Result.Failure(
                    DomainError.Validation("notification.mark_read.read_at_required", "ReadAtUtc is required"));
            }

            if (readAtUtc < CreatedAtUtc)
            {
                return Result.Failure(
                    DomainError.Validation("notification.mark_read.read_at_before_created", "ReadAtUtc cannot be earlier than CreatedAtUtc"));
            }

            if (IsRead)
            {
                return Result.Success();
            }

            IsRead = true;
            ReadAtUtc = readAtUtc;

            return Result.Success();
        }

        private static Result<string> NormalizeRequiredText(
            string value,
            int maxLength,
            string requiredCode,
            string requiredMessage,
            string tooLongCode,
            string tooLongMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result<string>.Failure(
                    DomainError.Validation(requiredCode, requiredMessage));
            }

            var trimmedValue = value.Trim();
            if (trimmedValue.Length > maxLength)
            {
                return Result<string>.Failure(
                    DomainError.Validation(tooLongCode, tooLongMessage));
            }

            return Result<string>.Success(trimmedValue);
        }

        private static Result<string?> NormalizeOptionalText(
            string? value,
            int maxLength,
            string tooLongCode,
            string tooLongMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result<string?>.Success(null);
            }

            var trimmedValue = value.Trim();
            if (trimmedValue.Length > maxLength)
            {
                return Result<string?>.Failure(
                    DomainError.Validation(tooLongCode, tooLongMessage));
            }

            return Result<string?>.Success(trimmedValue);
        }

        private static Result<string> NormalizeLinkUrl(string linkUrl)
        {
            if (string.IsNullOrWhiteSpace(linkUrl))
            {
                return Result<string>.Failure(
                    DomainError.Validation("notification.create.link_url_required", "LinkUrl is required"));
            }

            var trimmedLinkUrl = linkUrl.Trim();
            if (trimmedLinkUrl.Length > MaxLinkUrlLength)
            {
                return Result<string>.Failure(
                    DomainError.Validation("notification.create.link_url_too_long", $"LinkUrl cannot exceed {MaxLinkUrlLength} characters"));
            }

            if (!trimmedLinkUrl.StartsWith("/", StringComparison.Ordinal) ||
                trimmedLinkUrl.StartsWith("//", StringComparison.Ordinal) ||
                trimmedLinkUrl.Contains("://", StringComparison.Ordinal) ||
                trimmedLinkUrl.Contains("\\", StringComparison.Ordinal))
            {
                return Result<string>.Failure(
                    DomainError.Validation("notification.create.link_url_invalid", "LinkUrl must be an internal application path"));
            }

            if (trimmedLinkUrl.Any(char.IsWhiteSpace) ||
                trimmedLinkUrl.Any(char.IsControl))
            {
                return Result<string>.Failure(
                    DomainError.Validation("notification.create.link_url_invalid", "LinkUrl must not contain whitespace or control characters"));
            }

            return Result<string>.Success(trimmedLinkUrl);
        }
    }
}
