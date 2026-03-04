using FitLead.Api.Common.Claims;
using FitLead.Api.Contracts.Auth;
using FitLead.Api.Identity;
using FitLead.Application.Identity;
using FitLead.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitLead.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly SignInManager<AppIdentityUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthController(
            UserManager<AppIdentityUser> userManager,
            SignInManager<AppIdentityUser> signInManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
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

            return Ok(new LoginResponse(token.AccessToken, token.ExpiresIn, refresh.RefreshToken));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<RefreshResponse>> Refresh(
            [FromBody] RefreshRequest request)
        {
            var rotate = await _refreshTokenService.RotateAsync(
                request.RefreshToken,
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

            return Ok(new RefreshResponse(
                access.AccessToken,
                access.ExpiresIn,
                rotate.NewRefreshToken!));
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            await _refreshTokenService.RevokeFamilyByTokenAsync(
                request.RefreshToken,
                RefreshTokenRevocationReasons.Logout,
                HttpContext.RequestAborted);

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
