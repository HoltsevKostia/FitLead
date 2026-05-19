using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FitLead.Application.Common.Time;
using FitLead.Application.Media.Uploadcare;
using Microsoft.Extensions.Options;

namespace FitLead.Infrastructure.Media.Uploadcare
{
    public sealed class UploadcareUploadSignatureService : IUploadcareUploadSignatureService
    {
        private readonly UploadcareOptions _options;
        private readonly IClock _clock;

        public UploadcareUploadSignatureService(
            IOptions<UploadcareOptions> options,
            IClock clock)
        {
            _options = options.Value;
            _clock = clock;
        }

        public UploadcareUploadSignature Create()
        {
            var expiresAtUtc = new DateTimeOffset(
                _clock.UtcNow.AddMinutes(_options.UploadSignatureLifetimeMinutes));
            var secureExpire = expiresAtUtc.ToUnixTimeSeconds()
                .ToString(CultureInfo.InvariantCulture);
            var secureSignature = CreateSignature(secureExpire);

            return new UploadcareUploadSignature(secureSignature, secureExpire);
        }

        private string CreateSignature(string secureExpire)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SecretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(secureExpire));

            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
