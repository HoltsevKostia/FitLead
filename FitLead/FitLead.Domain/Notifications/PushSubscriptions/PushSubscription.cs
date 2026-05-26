using FitLead.Common.Domain;
using FitLead.Common.Results;
using DomainError = FitLead.Common.Errors.Error;

namespace FitLead.Domain.Notifications.PushSubscriptions
{
    public sealed class PushSubscription : AggregateRoot<Guid>
    {
        public const int MaxEndpointLength = 2048;
        public const int MaxKeyLength = 512;
        public const int MaxUserAgentLength = 500;

        public Guid UserId { get; private set; }
        public string Endpoint { get; private set; } = string.Empty;
        public string P256dh { get; private set; } = string.Empty;
        public string Auth { get; private set; } = string.Empty;
        public string? UserAgent { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? LastUsedAtUtc { get; private set; }
        public DateTime? RevokedAtUtc { get; private set; }

        private PushSubscription()
        {
        }

        private PushSubscription(
            Guid id,
            Guid userId,
            string endpoint,
            string p256dh,
            string auth,
            string? userAgent,
            DateTime createdAtUtc)
        {
            Id = id;
            UserId = userId;
            Endpoint = endpoint;
            P256dh = p256dh;
            Auth = auth;
            UserAgent = userAgent;
            CreatedAtUtc = createdAtUtc;
        }

        public static Result<PushSubscription> Create(
            Guid userId,
            string endpoint,
            string p256dh,
            string auth,
            string? userAgent,
            DateTime createdAtUtc)
        {
            if (userId == Guid.Empty)
            {
                return Result<PushSubscription>.Failure(
                    DomainError.Validation("push_subscription.create.user_id_required", "UserId is required"));
            }

            var endpointResult = NormalizeEndpoint(endpoint);
            if (endpointResult.IsFailure)
            {
                return Result<PushSubscription>.Failure(endpointResult.Error);
            }

            var p256dhResult = NormalizeRequiredValue(
                p256dh,
                MaxKeyLength,
                "push_subscription.create.p256dh_required",
                "P256dh is required",
                "push_subscription.create.p256dh_too_long",
                $"P256dh cannot exceed {MaxKeyLength} characters");
            if (p256dhResult.IsFailure)
            {
                return Result<PushSubscription>.Failure(p256dhResult.Error);
            }

            var authResult = NormalizeRequiredValue(
                auth,
                MaxKeyLength,
                "push_subscription.create.auth_required",
                "Auth is required",
                "push_subscription.create.auth_too_long",
                $"Auth cannot exceed {MaxKeyLength} characters");
            if (authResult.IsFailure)
            {
                return Result<PushSubscription>.Failure(authResult.Error);
            }

            var userAgentResult = NormalizeUserAgent(userAgent);
            if (userAgentResult.IsFailure)
            {
                return Result<PushSubscription>.Failure(userAgentResult.Error);
            }

            if (createdAtUtc == default)
            {
                return Result<PushSubscription>.Failure(
                    DomainError.Validation("push_subscription.create.created_at_required", "CreatedAtUtc is required"));
            }

            return Result<PushSubscription>.Success(
                new PushSubscription(
                    Guid.NewGuid(),
                    userId,
                    endpointResult.Value,
                    p256dhResult.Value,
                    authResult.Value,
                    userAgentResult.Value,
                    createdAtUtc));
        }

        public Result Refresh(
            Guid userId,
            string p256dh,
            string auth,
            string? userAgent)
        {
            if (userId == Guid.Empty)
            {
                return Result.Failure(
                    DomainError.Validation("push_subscription.refresh.user_id_required", "UserId is required"));
            }

            var p256dhResult = NormalizeRequiredValue(
                p256dh,
                MaxKeyLength,
                "push_subscription.refresh.p256dh_required",
                "P256dh is required",
                "push_subscription.refresh.p256dh_too_long",
                $"P256dh cannot exceed {MaxKeyLength} characters");
            if (p256dhResult.IsFailure)
            {
                return Result.Failure(p256dhResult.Error);
            }

            var authResult = NormalizeRequiredValue(
                auth,
                MaxKeyLength,
                "push_subscription.refresh.auth_required",
                "Auth is required",
                "push_subscription.refresh.auth_too_long",
                $"Auth cannot exceed {MaxKeyLength} characters");
            if (authResult.IsFailure)
            {
                return Result.Failure(authResult.Error);
            }

            var userAgentResult = NormalizeUserAgent(userAgent);
            if (userAgentResult.IsFailure)
            {
                return Result.Failure(userAgentResult.Error);
            }

            UserId = userId;
            P256dh = p256dhResult.Value;
            Auth = authResult.Value;
            UserAgent = userAgentResult.Value;
            RevokedAtUtc = null;

            return Result.Success();
        }

        public Result MarkUsed(DateTime usedAtUtc)
        {
            if (usedAtUtc == default)
            {
                return Result.Failure(
                    DomainError.Validation("push_subscription.mark_used.used_at_required", "UsedAtUtc is required"));
            }

            if (usedAtUtc < CreatedAtUtc)
            {
                return Result.Failure(
                    DomainError.Validation("push_subscription.mark_used.used_at_before_created", "UsedAtUtc cannot be earlier than CreatedAtUtc"));
            }

            LastUsedAtUtc = usedAtUtc;

            return Result.Success();
        }

        public Result Revoke(DateTime revokedAtUtc)
        {
            if (revokedAtUtc == default)
            {
                return Result.Failure(
                    DomainError.Validation("push_subscription.revoke.revoked_at_required", "RevokedAtUtc is required"));
            }

            if (revokedAtUtc < CreatedAtUtc)
            {
                return Result.Failure(
                    DomainError.Validation("push_subscription.revoke.revoked_at_before_created", "RevokedAtUtc cannot be earlier than CreatedAtUtc"));
            }

            RevokedAtUtc = revokedAtUtc;

            return Result.Success();
        }

        private static Result<string> NormalizeEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return Result<string>.Failure(
                    DomainError.Validation("push_subscription.create.endpoint_required", "Endpoint is required"));
            }

            var trimmedEndpoint = endpoint.Trim();
            if (trimmedEndpoint.Length > MaxEndpointLength)
            {
                return Result<string>.Failure(
                    DomainError.Validation("push_subscription.create.endpoint_too_long", $"Endpoint cannot exceed {MaxEndpointLength} characters"));
            }

            if (!Uri.TryCreate(trimmedEndpoint, UriKind.Absolute, out var uri) ||
                uri.Scheme != Uri.UriSchemeHttps)
            {
                return Result<string>.Failure(
                    DomainError.Validation("push_subscription.create.endpoint_invalid", "Endpoint must be an absolute HTTPS URL"));
            }

            return Result<string>.Success(trimmedEndpoint);
        }

        private static Result<string> NormalizeRequiredValue(
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

        private static Result<string?> NormalizeUserAgent(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                return Result<string?>.Success(null);
            }

            var trimmedUserAgent = userAgent.Trim();
            if (trimmedUserAgent.Length > MaxUserAgentLength)
            {
                return Result<string?>.Failure(
                    DomainError.Validation("push_subscription.create.user_agent_too_long", $"UserAgent cannot exceed {MaxUserAgentLength} characters"));
            }

            return Result<string?>.Success(trimmedUserAgent);
        }
    }
}
