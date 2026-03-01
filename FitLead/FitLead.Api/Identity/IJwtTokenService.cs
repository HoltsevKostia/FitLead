using FitLead.Infrastructure.Identity;

namespace FitLead.Api.Identity
{
    public interface IJwtTokenService
    {
        AccessTokenResult CreateAccessToken(AppIdentityUser user);
    }
}
