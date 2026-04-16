using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace FitLead.Api.Common.Claims
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetSub(this ClaimsPrincipal user)
            => user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        public static string? GetEmail(this ClaimsPrincipal user)
            => user.FindFirstValue(JwtRegisteredClaimNames.Email);
        public static string? GetJti(this ClaimsPrincipal user)
            => user.FindFirstValue(JwtRegisteredClaimNames.Jti);
    }
}
