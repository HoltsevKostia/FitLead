using FitLead.Application.Identity;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using FitLead.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace FitLead.Api.Identity
{
    public sealed class AuthTokenIssuer : IAuthTokenIssuer
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthTokenIssuer(
            UserManager<AppIdentityUser> userManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<Result<AuthTokensResult>> IssueAsync(
            string identityUserId,
            string businessRole,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(identityUserId);
            if (user is null)
            {
                return Result<AuthTokensResult>.Failure(
                    Error.NotFound("auth.identity_user_not_found", "Identity user not found"));
            }

            var access = _jwtTokenService.CreateAccessToken(
                user,
                new[]
                {
                    new Claim(ClaimTypes.Role, businessRole)
                });

            var refresh = await _refreshTokenService.IssueForLoginAsync(identityUserId, cancellationToken);

            return Result<AuthTokensResult>.Success(
                new AuthTokensResult(
                    access.AccessToken,
                    access.ExpiresIn,
                    refresh.RefreshToken));
        }
    }
}
