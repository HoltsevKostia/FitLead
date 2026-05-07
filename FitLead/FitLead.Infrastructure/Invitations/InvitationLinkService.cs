using FitLead.Application.Invitations.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace FitLead.Infrastructure.Invitations
{
    public sealed class InvitationLinkService : IInvitationLinkService
    {
        private readonly IConfiguration _configuration;

        public InvitationLinkService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public InvitationLinkPayload CreateLink()
        {
            var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
            var tokenHash = ComputeTokenHash(token);

            return new InvitationLinkPayload(
                token,
                tokenHash,
                BuildInviteUrl(token));
        }

        public string ComputeTokenHash(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            return Convert.ToHexString(SHA256.HashData(tokenBytes));
        }

        private string BuildInviteUrl(string token)
        {
            var baseUrl = _configuration["ClientApp:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return $"/invite/{token}";
            }

            return $"{baseUrl.TrimEnd('/')}/invite/{token}";
        }
    }
}
