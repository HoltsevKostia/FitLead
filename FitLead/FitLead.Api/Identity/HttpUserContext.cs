using FitLead.Application.Common.Identity;
using System.Security.Claims;


namespace FitLead.Infrastructure.Identity
{
    public sealed class HttpUserContext : IUserContext
    {
        private const string DevHeaderUserId = "X-User-Id";
        private readonly IHttpContextAccessor _http;

        public HttpUserContext(IHttpContextAccessor http) => _http = http;

        public bool IsAuthenticated =>
            _http.HttpContext?.User?.Identity?.IsAuthenticated == true;

        public Guid UserId =>
            UserIdOrNull ?? throw new UnauthorizedAccessException("UserId is not available");

        public Guid? UserIdOrNull
        {
            get
            {
                var ctx = _http.HttpContext;
                if (ctx is null) return null;

                // Future path (post-MVP): JWT/Identity/IdP
                // - NameIdentifier часто мапиться на sub, але залежить від налаштувань
                var claim =
                    ctx.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    ctx.User.FindFirstValue("sub") ??
                    ctx.User.FindFirstValue("user_id");

                if (Guid.TryParse(claim, out var idFromClaims))
                    return idFromClaims;

                // MVP/testing path: header
                if (ctx.Request.Headers.TryGetValue(DevHeaderUserId, out var header) &&
                    Guid.TryParse(header.ToString(), out var idFromHeader))
                    return idFromHeader;

                return null;
            }
        }
    }
}
