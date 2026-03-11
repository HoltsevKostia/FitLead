using FitLead.Application.Identity;
using FitLead.Common.Errors;
using FitLead.Common.Results;
using Microsoft.AspNetCore.Identity;

namespace FitLead.Infrastructure.Identity
{
    public sealed class IdentityAccountService : IIdentityAccountService
    {
        private readonly UserManager<AppIdentityUser> _userManager;

        public IdentityAccountService(UserManager<AppIdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user is not null;
        }

        public async Task<Result<IdentityProvisionResult>> CreateWithRoleAsync(
            string email,
            string password,
            string role,
            CancellationToken cancellationToken)
        {
            var identityUser = new AppIdentityUser
            {
                Email = email,
                UserName = email
            };

            var createResult = await _userManager.CreateAsync(identityUser, password);
            if (!createResult.Succeeded)
                return Result<IdentityProvisionResult>.Failure(ToError(createResult, "auth.identity_create_failed"));

            var roleResult = await _userManager.AddToRoleAsync(identityUser, role);
            if (!roleResult.Succeeded)
                return Result<IdentityProvisionResult>.Failure(ToError(roleResult, "auth.role_assignment_failed"));

            return Result<IdentityProvisionResult>.Success(
                new IdentityProvisionResult(identityUser.Id, identityUser.Email ?? email));
        }

        private static Error ToError(IdentityResult result, string fallbackCode)
        {
            var errors = result.Errors.ToList();
            if (errors.Count == 0)
                return Error.Validation(fallbackCode, "Identity operation failed");

            var first = errors[0];
            if (string.Equals(first.Code, "DuplicateEmail", StringComparison.OrdinalIgnoreCase))
                return Error.Conflict("auth.email_exists", "User with this email already exists");

            var metadata = new Dictionary<string, object?>
            {
                ["identityErrors"] = errors.Select(e => new { e.Code, e.Description }).ToList()
            };

            return Error.Validation("auth.identity_validation_failed", first.Description, metadata);
        }
    }
}
