using FitLead.Infrastructure.Identity;
using System.Security.Claims;

namespace FitLead.Api.Identity
{
    public interface IJwtTokenService
    {
        AccessTokenResult CreateAccessToken(
            AppIdentityUser user,
            IEnumerable<Claim>? additionalClaims = null);
    }
}
