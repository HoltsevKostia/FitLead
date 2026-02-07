using FitLead.Application.Common.Deletion;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text.Json;

namespace FitLead.Infrastructure.Deletion
{
    public sealed class DataProtectionDeletionConfirmationTokenService : IDeletionConfirmationTokenService
    {
        private readonly ITimeLimitedDataProtector _protector;
        private readonly TimeSpan _lifetime;

        public DataProtectionDeletionConfirmationTokenService(
            IDataProtectionProvider provider,
            IOptions<DeletionTokenOptions> options)
        {
            _lifetime = TimeSpan.FromMinutes(options.Value.LifetimeMinutes <= 0 ? 15 : options.Value.LifetimeMinutes);

            _protector = provider
                .CreateProtector("FitLead.DeletionConfirmationToken.v1")
                .ToTimeLimitedDataProtector();
        }

        public string IssueToken(DeletionScope scope, Guid targetId, int usageCount, DateTime utcNow)
        {
            var payload = new DeletionTokenPayload(scope, targetId, usageCount, utcNow);
            var json = JsonSerializer.Serialize(payload);

            return _protector.Protect(json, _lifetime);
        }

        public bool TryValidateToken(
            string token,
            DeletionScope expectedScope,
            Guid expectedTargetId,
            DateTime utcNow,
            out DeletionTokenPayload payload)
        {
            payload = default!;

            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                var json = _protector.Unprotect(token);

                var parsed = JsonSerializer.Deserialize<DeletionTokenPayload>(json);
                if (parsed is null)
                    return false;

                if (parsed.Scope != expectedScope)
                    return false;

                if (parsed.TargetId != expectedTargetId)
                    return false;

                if (parsed.IssuedAtUtc > utcNow.AddMinutes(1))
                    return false;

                payload = parsed;
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
