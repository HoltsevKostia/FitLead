using FitLead.Application.Identity;
using System.Security.Claims;

namespace FitLead.Api.Identity
{
    public sealed class HttpUserContext : IUserContext
    {
        private readonly IHttpContextAccessor _http;

        public HttpUserContext(IHttpContextAccessor http) => _http = http;

        public bool IsAuthenticated => UserIdOrNull.HasValue;

        public Guid UserId =>
            UserIdOrNull ?? throw new UnauthorizedAccessException("Domain UserId is not available");

        public Guid? UserIdOrNull
        {
            get
            {
                var ctx = _http.HttpContext;
                if (ctx is null)
                    return null;

                var claim = ctx.User.FindFirstValue(CustomClaimTypes.DomainUserId);
                if (Guid.TryParse(claim, out var value))
                    return value;

                return null;
            }
        }
    }
}
