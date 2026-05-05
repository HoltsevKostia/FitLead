using FitLead.Api.Auth.Contracts;
using FitLead.Api.Common.Claims;
using FitLead.Api.Common.Results;
using FitLead.Api.Identity;
using FitLead.Application.Identity;
using FitLead.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace FitLead.Api.Auth
{
    [ApiController]
    [Route("auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly SignInManager<AppIdentityUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IMediator _mediator;
        private readonly JwtOptions _jwtOptions;

        public AuthController(
            UserManager<AppIdentityUser> userManager,
            SignInManager<AppIdentityUser> signInManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            IMediator mediator,
            IOptions<JwtOptions> jwtOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
            _mediator = mediator;
            _jwtOptions = jwtOptions.Value;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _mediator.Send(
                new RegisterUserCommand(
                    request.Email,
                    request.Password,
                    request.FullName,
                    request.Role));

            if (result.IsFailure)
                return result.ToActionResult(this);

            AppendAuthCookies(result.Value.AccessToken, result.Value.ExpiresIn, result.Value.RefreshToken);

            return StatusCode(
                StatusCodes.Status201Created,
                new AuthSessionResponse(result.Value.ExpiresIn));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthSessionResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Unauthorized();

            var signInResult = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

            if (!signInResult.Succeeded)
                return Unauthorized();

            var role = await GetSingleBusinessRoleOrNullAsync(user);
            if (role is null)
                return Unauthorized();

            var token = _jwtTokenService.CreateAccessToken(
                user,
                new[]
                {
                    new Claim(ClaimTypes.Role, role)
                });

            var refresh = await _refreshTokenService.IssueForLoginAsync(
                user.Id,
                HttpContext.RequestAborted);

            AppendAuthCookies(token.AccessToken, token.ExpiresIn, refresh.RefreshToken);

            return Ok(new AuthSessionResponse(token.ExpiresIn));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthSessionResponse>> Refresh()
        {
            var refreshToken = Request.Cookies[AuthCookieNames.RefreshToken];
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized();

            var rotate = await _refreshTokenService.RotateAsync(
                refreshToken,
                HttpContext.RequestAborted);

            if (!rotate.Success || string.IsNullOrWhiteSpace(rotate.NewRefreshToken) 
                || string.IsNullOrWhiteSpace(rotate.IdentityUserId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(rotate.IdentityUserId);
            if (user is null)
                return Unauthorized();

            var role = await GetSingleBusinessRoleOrNullAsync(user);
            if (role is null)
                return Unauthorized();

            var access = _jwtTokenService.CreateAccessToken(
                user,
                new[]
                {
                    new Claim(ClaimTypes.Role, role)
                });

            AppendAuthCookies(access.AccessToken, access.ExpiresIn, rotate.NewRefreshToken!);

            return Ok(new AuthSessionResponse(access.ExpiresIn));
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies[AuthCookieNames.RefreshToken];
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await _refreshTokenService.RevokeFamilyByTokenAsync(
                    refreshToken,
                    RefreshTokenRevocationReasons.Logout,
                    HttpContext.RequestAborted);
            }

            DeleteAuthCookies();

            return NoContent();
        }

        [HttpGet("current-user")]
        [Authorize]
        public IActionResult GetClaims()
        {
            return Ok(new
            {
                sub = User.GetSub(),
                email = User.GetEmail(),
                jti = User.GetJti()
            });
        }

        private void AppendAuthCookies(
            string accessToken,
            int accessTokenExpiresInSeconds,
            string refreshToken)
        {
            Response.Cookies.Append(
                AuthCookieNames.AccessToken,
                accessToken,
                CreateCookieOptions(
                    DateTimeOffset.UtcNow.AddSeconds(accessTokenExpiresInSeconds),
                    path: "/"));

            Response.Cookies.Append(
                AuthCookieNames.RefreshToken,
                refreshToken,
                CreateCookieOptions(
                    DateTimeOffset.UtcNow.AddDays(Math.Max(1, _jwtOptions.RefreshTokenDays)),
                    path: "/auth"));
        }

        private void DeleteAuthCookies()
        {
            Response.Cookies.Delete(
                AuthCookieNames.AccessToken,
                CreateCookieOptions(DateTimeOffset.UtcNow.AddDays(-1), path: "/"));

            Response.Cookies.Delete(
                AuthCookieNames.RefreshToken,
                CreateCookieOptions(DateTimeOffset.UtcNow.AddDays(-1), path: "/auth"));
        }

        private CookieOptions CreateCookieOptions(
            DateTimeOffset expiresAt,
            string path)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = expiresAt,
                Path = path,
                IsEssential = true
            };
        }

        private async Task<string?> GetSingleBusinessRoleOrNullAsync(AppIdentityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var businessRoles = roles
                .Where(r => r is "Trainer" or "Client")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (businessRoles.Count != 1)
                return null;

            return businessRoles[0];
        }
    }
}
