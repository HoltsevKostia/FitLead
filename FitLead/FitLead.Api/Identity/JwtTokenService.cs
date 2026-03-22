using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FitLead.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FitLead.Api.Identity
{
    public sealed class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _options;
        private readonly SigningCredentials _signingCredentials;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
            JwtSigningKeyResolver.Validate(_options);
            _signingCredentials = JwtSigningKeyResolver.CreateSigningCredentials(_options);
        }

        public AccessTokenResult CreateAccessToken(
            AppIdentityUser user,
            IEnumerable<Claim>? additionalClaims = null)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrWhiteSpace(user.Email))
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));

            if (additionalClaims is not null)
                claims.AddRange(additionalClaims);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: _signingCredentials);

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);
            var expiresIn = (int)Math.Max(1, (expiresAt - DateTime.UtcNow).TotalSeconds);

            return new AccessTokenResult(encodedToken, expiresIn);
        }
    }
}
