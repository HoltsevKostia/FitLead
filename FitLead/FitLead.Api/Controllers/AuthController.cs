using FitLead.Api.Common.Claims;
using FitLead.Api.Contracts.Auth;
using FitLead.Api.Identity;
using FitLead.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FitLead.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly SignInManager<AppIdentityUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(
            UserManager<AppIdentityUser> userManager,
            SignInManager<AppIdentityUser> signInManager,
            IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login(
            [FromBody] LoginRequest request)
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

            var token = _jwtTokenService.CreateAccessToken(user);

            return Ok(new LoginResponse(token.AccessToken, token.ExpiresIn));
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
    }
}
