using FitLead.Application.Common.Identity;
using System.Security.Claims;

namespace FitLead.Api.Identity
{
    public sealed class HttpUserContext : IUserContext
    {
        private const string DevHeaderIdentityUserId = "X-Identity-User-Id";
        private readonly IHttpContextAccessor _http;

        public HttpUserContext(IHttpContextAccessor http) => _http = http;

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(IdentityUserIdOrNull);

        public string IdentityUserId =>
            IdentityUserIdOrNull ?? throw new UnauthorizedAccessException("IdentityUserId is not available");

        public string? IdentityUserIdOrNull
        {
            get
            {
                var ctx = _http.HttpContext;
                if (ctx is null) return null;

                var identityUserId =
                    ctx.User.FindFirstValue("sub") ??
                    ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrWhiteSpace(identityUserId))
                    return identityUserId;

                // Temporary dev backdoor until all flows move to JWT.
                if (ctx.Request.Headers.TryGetValue(DevHeaderIdentityUserId, out var header))
                {
                    var raw = header.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(raw))
                        return raw;
                }

                return null;
            }
        }

        public Guid UserId =>
            UserIdOrNull ?? throw new UnauthorizedAccessException("Domain UserId is not available");

        public Guid? UserIdOrNull
        {
            get
            {
                var identityUserId = IdentityUserIdOrNull;
                if (Guid.TryParse(identityUserId, out var value))
                    return value;

                return null;
            }
        }
    }
}
